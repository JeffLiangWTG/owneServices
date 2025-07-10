using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public partial class DepartmentChooser
	{
		protected DepartmentChooser(BusinessObjectFactory factory)
		{
			this.Factory = factory;
			DepartmentCodes = new Dictionary<KeyForDictionary, Func<ZGuid>>();
			DefaultDepartmentsCollection = new Dictionary<KeyForDictionary, Func<JobInvoicingDefaultDepartmentsCollection>>();
			CustomDepartmentChooser = new CustomDepartmentChooser(factory);
			InitDepartmentDictionary();
		}

		public static DepartmentChooser New(BusinessObjectFactory factory)
		{
			var overridden = OverridableNewDelegate.Value;
			if (overridden == null)
			{
				return new DepartmentChooser(factory);
			}
			else
			{
				return overridden(factory);
			}
		}

		protected delegate DepartmentChooser NewDelegate(BusinessObjectFactory factory);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		internal const string Anything = "Anything";
		internal const string Excise = "EXC";
		internal const string Post = "PST";

		// This signature is only used from Custom DSB ChargePoster due to declaration plugged into shipment for IsImport
		public ZGuid GetDepartment(IJobInvoicingPlugIn parentBusinessObject, bool isOriginLocal, bool isDestinationForeign, ZString containerMode)
		{
			ZString consumerType = parentBusinessObject.InvoicingSupporter.ConsumerType.Code;

			if (UseDefaultLoginDepartment(consumerType))
			{
				return GetDepartmentBasedOnCurrentLogin();
			}
			else
			{
				ZString transportMode = parentBusinessObject.InvoicingSupporter.TransportMode;
				if (CheckIfDepartmentBasedOnMessageType(parentBusinessObject))
				{
					transportMode = ((BaseJobDeclaration)parentBusinessObject).JE_MessageType;
				}

				return GetDepartmentFromDictionary(GetDepartmentDictionaryKey(consumerType, isOriginLocal, isDestinationForeign, transportMode, containerMode));
			}
		}

		// Only used from Consol Invoice Poster
		public ZGuid GetDepartment(JobInvoicingConsumerType consumerType, ZString origin, ZString destination, ZString transportMode, ZString containerMode)
		{
			return GetDepartmentCore(consumerType, origin, destination, transportMode, containerMode, false);
		}

		/// <summary>
		/// Makes decision on which department is going to be used for this Parent Business object.
		/// </summary>
		/// <param name="parentBusinessObject">Parent business object (Shipment, Declaration, etc)</param>
		/// <returns>PK of the department</returns>
		public ZGuid GetDepartment(IJobInvoicingPlugIn parentBusinessObject)
		{
			if (parentBusinessObject != null)
			{
				if (AccountingConfigurationRegistry.Instance.CustomDepartmentDefaultingRuleEngineConfiguration.Value)
				{
					var departmentDefaultingManager = ObjectFactory.Get<IJobBillingDepartmentDefaultingManager>();
					departmentDefaultingManager.SetDefaultValue(parentBusinessObject, Env.CurrentCompany, Env.CurrentBranch, Env.CurrentDepartment);
					if (departmentDefaultingManager.DefaultValue != null)
					{
						var result = (ZGuid)departmentDefaultingManager.DefaultValue;
						if (!result.IsEmpty)
						{
							return result;
						}
					}
				}
				var departmentCode = CustomDepartmentChooser.GetCodeWithConfiguration(parentBusinessObject);
				var consumerType = parentBusinessObject.InvoicingSupporter?.ConsumerType;

				if (!departmentCode.IsEmpty)
				{
					return GetDepartmentForCustomConfiguration(departmentCode);
				}
				else if (consumerType != null && UseDefaultLoginDepartment(consumerType?.Code))
				{
					return GetDepartmentBasedOnCurrentLogin();
				}
				else if (consumerType?.Code == JobInvoicingConsumerTypes.TransportConsignment.Code)
				{
					return AccountingConfigurationRegistry.Instance.LandTransportJobsDefaultDept.Value;
				}
				else if (!parentBusinessObject.InvoicingSupporter.OverriddenDepartmentPK.IsEmpty && parentBusinessObject.InvoicingSupporter.OverriddenDepartmentPK.IsValid)
				{
					return parentBusinessObject.InvoicingSupporter.OverriddenDepartmentPK;
				}
				else if (ConsumerTypeHasOnlyOneDepartment(consumerType))
				{
					return GetDepartmentCore(consumerType, new ZString((NoResString)"Origin"), new ZString((NoResString)"Destination"), new ZString("TransportMode"), new ZString("ContainerMode"), parentBusinessObject.InvoicingSupporter.IsImport);
				}
				else if (CheckIfDepartmentBasedOnMessageType(parentBusinessObject))
				{
					return GetDepartmentFromDictionary(GetDepartmentDictionaryKey(consumerType?.Code, false, false, ((BaseJobDeclaration)parentBusinessObject).JE_MessageType, Anything));
				}
				else if (parentBusinessObject.InvoicingSupporter.Origin != null && parentBusinessObject.InvoicingSupporter.Destination != null)
				{
					return GetDepartmentCore(consumerType, parentBusinessObject.InvoicingSupporter.ConsolType, parentBusinessObject.InvoicingSupporter.Origin.Code, parentBusinessObject.InvoicingSupporter.Destination.Code, parentBusinessObject.InvoicingSupporter.TransportMode, parentBusinessObject.InvoicingSupporter.ContainerMode, parentBusinessObject.InvoicingSupporter.IsImport);
				}
				else if (consumerType?.Code == JobInvoicingConsumerTypes.Brokerage.Code)
				{
					return GetDepartmentCore(consumerType, "", "", parentBusinessObject.InvoicingSupporter.TransportMode, parentBusinessObject.InvoicingSupporter.ContainerMode, parentBusinessObject.InvoicingSupporter.IsImport);
				}
				else if (consumerType?.Code == JobInvoicingConsumerTypes.CustomsTransitNCTS.Code)
				{
					if (!EnvProxy.Instance.CurrentUser.IsWebUser)
					{
						var isNCTSPhase4 = parentBusinessObject.InvoicingSupporter?.IsNCTSPhase4 ?? false;
						if ((isNCTSPhase4 && AccountingConfigurationRegistry.Instance.CustomsDefaultToCurrentLoginDept.Value) || (!isNCTSPhase4 && AccountingConfigurationRegistry.Instance.NCTSDefaultToCurrentLoginDept.Value))
						{
							return GetDepartmentBasedOnCurrentLogin();
						}
					}
					return GetDepartmentCore(consumerType, "", "", parentBusinessObject.InvoicingSupporter.TransportMode, parentBusinessObject.InvoicingSupporter.ContainerMode, parentBusinessObject.InvoicingSupporter.IsImport);
				}
			}
			return ZGuid.Empty;
		}

		ZGuid GetDepartmentForCustomConfiguration(ZString departmentCode)
		{
			var departmentBizObj = GetDepartmentBizObjFromDeptCode(Factory, departmentCode);
			if (departmentBizObj != null && departmentBizObj.GE_IsActive)
			{
				return departmentBizObj.PK;
			}
			else
			{
				return ZGuid.Empty;
			}
		}

		internal static GlbDepartment GetDepartmentBizObjFromDeptCode(BusinessObjectFactory factory, ZString departmentCode)
		{
			GlbDepartment department = null;
			if (factory != null && !departmentCode.IsEmpty)
			{
				ZQuery query = new ZQuery(GlbDepartmentSchema.GE_Code, departmentCode);
				department = factory.LoadTop1<GlbDepartment>(query);
			}
			return department;
		}

		ZGuid GetDepartmentCore(JobInvoicingConsumerType consumerType, ZString origin, ZString destination, ZString transportMode, ZString containerMode, bool isImport)
		{
			return GetDepartmentCore(consumerType, Constants.JobInvoicingDefaultDepartmentConsolType.NoConsol, origin, destination, transportMode, containerMode, isImport);
		}

		ZGuid GetDepartmentCore(JobInvoicingConsumerType consumerType, ZString consolType, ZString origin, ZString destination, ZString transportMode, ZString containerMode, bool isImport)
		{
			var result = ZGuid.Empty;
			if (consumerType != null)
			{
				if (consumerType.Code == JobInvoicingConsumerTypes.OneOffQuotation.Code || consumerType.Code == JobInvoicingConsumerTypes.QuotedBooking.Code)
				{
					consumerType = JobInvoicingConsumerTypes.Shipment;
				}

				KeyForDictionary key;
				bool isOriginLocal = GlbCompany.CurrentCompany.GC_RN_NKCountryCode == origin.SubstringSafe(0, 2) ||
										(GlbCompany.CurrentCompany.GC_RN_NKCountryCode != destination.SubstringSafe(0, 2) &&
											!ImportExportHelper.IsInCommunityRegion(destination) &&
											ImportExportHelper.IsInCommunityRegion(origin));

				bool isDestinationForeign = GlbCompany.CurrentCompany.GC_RN_NKCountryCode != destination.SubstringSafe(0, 2)
											&& (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == origin.SubstringSafe(0, 2) ||
											ImportExportHelper.IsInCommunityRegion(origin) ||
											!ImportExportHelper.IsInCommunityRegion(destination));

				// CFS Shipment & Load List
				if (consumerType.Code == JobInvoicingConsumerTypes.CFSShipment.Code || consumerType.Code == JobInvoicingConsumerTypes.CFSLoadList.Code)
				{
					containerMode = Anything;
				}

				// Declaration
				if (consumerType.Code == JobInvoicingConsumerTypes.Brokerage.Code)
				{
					if (origin.IsEmpty || destination.IsEmpty)
					{
						if (isImport)
						{
							isOriginLocal = false;
							isDestinationForeign = false;
						}
						else
						{
							isOriginLocal = true;
							isDestinationForeign = true;
						}
					}
				}

				// MasterAWB, Warehouse, CusMAWB, CusUnderbond, CTOCusMAWB
				if (ConsumerTypeHasOnlyOneDepartment(consumerType))
				{
					isOriginLocal = false;
					isDestinationForeign = false;
					transportMode = Anything;
					containerMode = Anything;
				}

				if (consumerType.Code == JobInvoicingConsumerTypes.AgencyBooking.Code || consumerType.Code == JobInvoicingConsumerTypes.AgencyBillOfLading.Code)
				{
					key = GetShippingDepartmentDictionaryKey(consumerType.Code, isOriginLocal, isDestinationForeign, containerMode);
				}
				else
				{
					key = GetDepartmentDictionaryKey(consumerType.Code, consolType, isOriginLocal, isDestinationForeign, transportMode, containerMode);
				}

				result = GetDepartmentFromDictionary(key);
			}
			return result;
		}

#if DEBUG
		internal
#endif
		protected struct KeyForDictionary
		{
			public ZString TableName;
			public ZString ConsolType;
			public bool IsOriginLocal;
			public bool IsDestinationForeign;
			public ZString TransportMode;
			public ZString ContainerMode;

			public override bool Equals(object obj)
			{
				var result = false;
				if (obj is KeyForDictionary)
				{
					KeyForDictionary key = (KeyForDictionary)obj;
					result = this.TableName == key.TableName && this.ConsolType == key.ConsolType && this.IsOriginLocal == key.IsOriginLocal && this.IsDestinationForeign == key.IsDestinationForeign &&
						this.TransportMode == key.TransportMode && this.ContainerMode == key.ContainerMode;
				}
				return result;
			}

			public override int GetHashCode()
			{
				return TableName.GetHashCode() ^ ConsolType.GetHashCode() ^ IsOriginLocal.GetHashCode() ^ IsDestinationForeign.GetHashCode() ^ TransportMode.GetHashCode() ^ ContainerMode.GetHashCode();
			}
		}

		bool CheckIfDepartmentBasedOnMessageType(IJobInvoicingPlugIn jobInvoicingPlugIn)
		{
			bool result = false;
			BaseJobDeclaration declaration = jobInvoicingPlugIn as BaseJobDeclaration;
			if (declaration != null &&
				(declaration.JE_MessageType == JobMessageTypeList.Codes.Drawback ||
				declaration.JE_MessageType == JobMessageTypeList.Codes.Refund ||
				declaration.JE_MessageType == JobMessageTypeList.Codes.MiscellaneousCustoms ||
				declaration.JE_MessageType == JobMessageTypeList.Codes.ExWarehouse ||
				declaration.JE_MessageType == Excise))
			{
				result = true;
			}
			return result;
		}

		protected bool ConsumerTypeHasOnlyOneDepartment(JobInvoicingConsumerType consumerType)
		{
			return (consumerType != null &&
					(consumerType.Code != null &&
					(consumerType.Code == JobInvoicingConsumerTypes.WarehouseInwards.Code ||
					consumerType.Code == JobInvoicingConsumerTypes.WarehouseOutwards.Code ||
					consumerType.Code == JobInvoicingConsumerTypes.TransitReceive.Code ||
					consumerType.Code == JobInvoicingConsumerTypes.TransitDispatch.Code ||
					consumerType.Code == JobInvoicingConsumerTypes.TransitReceiveTransportationUnit.Code ||
					consumerType.Code == JobInvoicingConsumerTypes.TransitDispatchLoadList.Code ||
					consumerType.Code == JobInvoicingConsumerTypes.TransitDispatchTransportationUnit.Code ||
					consumerType.Code == JobInvoicingConsumerTypes.WarehouseStorage.Code ||
					consumerType.Code == JobInvoicingConsumerTypes.WarehouseStocktake.Code ||
					consumerType.Code == JobInvoicingConsumerTypes.MasterAWB.Code ||
					consumerType.Code == JobInvoicingConsumerTypes.CusMAWB.Code ||
					consumerType.Code == JobInvoicingConsumerTypes.CusUnderbond.Code ||
					consumerType.Code == JobInvoicingConsumerTypes.CTOCusMAWB.Code ||
					consumerType.Code == JobInvoicingConsumerTypes.CTOCusImportHAWB.Code ||
					consumerType.Code == JobInvoicingConsumerTypes.CTOCusExportHAWB.Code ||
					consumerType.Code == JobInvoicingConsumerTypes.AgencyVoyageAccounting.Code ||
					consumerType.Code == JobInvoicingConsumerTypes.AgencySundryCharges.Code ||
					consumerType.Code == JobInvoicingConsumerTypes.WorkItem.Code ||
					consumerType.Code == JobInvoicingConsumerTypes.WarehouseAdHocServiceJob.Code ||
					consumerType.Code == JobInvoicingConsumerTypes.Project.Code ||
					consumerType.Code == JobInvoicingConsumerTypes.WorkRequest.Code ||
					consumerType == JobInvoicingConsumerTypes.ImporterSecurityFiling ||
					consumerType.Code == JobInvoicingConsumerTypes.FCLStorage.Code ||
					consumerType.Code == JobInvoicingConsumerTypes.CYDReceiveAdvice.Code ||
					consumerType.Code == JobInvoicingConsumerTypes.CYDReleaseAdvice.Code ||
					consumerType.Code == JobInvoicingConsumerTypes.CYDTransportationUnit.Code ||
					consumerType.Code == JobInvoicingConsumerTypes.CYDAdHocServiceOrder.Code ||
					consumerType.Code == JobInvoicingConsumerTypes.MNRWorkOrderHeader.Code ||
					consumerType.Code == JobInvoicingConsumerTypes.CYDPeriodicInvoicing.Code ||
					consumerType.Code == JobInvoicingConsumerTypes.WarehouseVASOrder.Code ||
					consumerType.Code == JobInvoicingConsumerTypes.BRLPCO.Code
					)));
		}

#region Implementation
		protected Dictionary<KeyForDictionary, Func<ZGuid>> DepartmentCodes;
		protected Dictionary<KeyForDictionary, Func<JobInvoicingDefaultDepartmentsCollection>> DefaultDepartmentsCollection;
		protected BusinessObjectFactory Factory;
		protected string JobContainerTableName = "JobContainer";
		readonly CustomDepartmentChooser CustomDepartmentChooser;

		protected void InitDepartmentDictionary()
		{
			// FORWARDING SHIPMENTS
			DefaultDepartmentsCollection.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Shipment.Code, true, true, Constants.TransportModes.Sea, Constants.ContainerModes.FCL), () => AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingExportSeaFcl.Value);
			DefaultDepartmentsCollection.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Shipment.Code, true, true, Constants.TransportModes.Sea, Constants.ContainerModes.LCL), () => AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingExportSeaLcl.Value);
			DefaultDepartmentsCollection.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Shipment.Code, true, true, Constants.TransportModes.Sea, Anything), () => AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingExportSeaOther.Value);
			DefaultDepartmentsCollection.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Shipment.Code, true, true, Constants.TransportModes.Air, Constants.ContainerModes.ULD), () => AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingExportAirULD.Value);
			DefaultDepartmentsCollection.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Shipment.Code, true, true, Constants.TransportModes.SeaAir, Anything), () => AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingExportAirOther.Value);
			DefaultDepartmentsCollection.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Shipment.Code, true, true, Constants.TransportModes.AirSea, Anything), () => AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingExportAirOther.Value);
			DefaultDepartmentsCollection.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Shipment.Code, true, true, Constants.TransportModes.Courier, Anything), () => AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingExportAirOther.Value);
			DefaultDepartmentsCollection.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Shipment.Code, true, true, Constants.TransportModes.Air, Anything), () => AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingExportAirOther.Value);
			DefaultDepartmentsCollection.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Shipment.Code, true, true, Constants.TransportModes.Rail, Constants.ContainerModes.FCL), () => AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingExportRailFCL.Value);
			DefaultDepartmentsCollection.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Shipment.Code, true, true, Constants.TransportModes.Rail, Constants.ContainerModes.LCL), () => AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingExportRailLCL.Value);
			DefaultDepartmentsCollection.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Shipment.Code, true, true, Constants.TransportModes.Rail, Anything), () => AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingExportRail.Value);
			DefaultDepartmentsCollection.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Shipment.Code, true, true, Constants.TransportModes.Road, Anything), () => AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingExportRoad.Value);

			DefaultDepartmentsCollection.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Shipment.Code, false, false, Constants.TransportModes.Sea, Constants.ContainerModes.FCL), () => AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingImportSeaFcl.Value);
			DefaultDepartmentsCollection.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Shipment.Code, false, false, Constants.TransportModes.Sea, Constants.ContainerModes.LCL), () => AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingImportSeaLcl.Value);
			DefaultDepartmentsCollection.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Shipment.Code, false, false, Constants.TransportModes.Sea, Anything), () => AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingImportSeaOther.Value);
			DefaultDepartmentsCollection.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Shipment.Code, false, false, Constants.TransportModes.Air, Constants.ContainerModes.ULD), () => AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingImportAirULD.Value);
			DefaultDepartmentsCollection.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Shipment.Code, false, false, Constants.TransportModes.SeaAir, Anything), () => AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingImportAirOther.Value);
			DefaultDepartmentsCollection.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Shipment.Code, false, false, Constants.TransportModes.AirSea, Anything), () => AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingImportAirOther.Value);
			DefaultDepartmentsCollection.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Shipment.Code, false, false, Constants.TransportModes.Courier, Anything), () => AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingImportAirOther.Value);
			DefaultDepartmentsCollection.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Shipment.Code, false, false, Constants.TransportModes.Air, Anything), () => AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingImportAirOther.Value);
			DefaultDepartmentsCollection.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Shipment.Code, false, false, Constants.TransportModes.Rail, Constants.ContainerModes.FCL), () => AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingImportRailFCL.Value);
			DefaultDepartmentsCollection.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Shipment.Code, false, false, Constants.TransportModes.Rail, Constants.ContainerModes.LCL), () => AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingImportRailLCL.Value);
			DefaultDepartmentsCollection.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Shipment.Code, false, false, Constants.TransportModes.Rail, Anything), () => AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingImportRail.Value);
			DefaultDepartmentsCollection.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Shipment.Code, false, false, Constants.TransportModes.Road, Anything), () => AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingImportRoad.Value);

			DefaultDepartmentsCollection.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Shipment.Code, true, false, Constants.TransportModes.Sea, Constants.ContainerModes.FCL), () => AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingDomesticSeaFcl.Value);
			DefaultDepartmentsCollection.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Shipment.Code, true, false, Constants.TransportModes.Sea, Constants.ContainerModes.LCL), () => AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingDomesticSeaLcl.Value);
			DefaultDepartmentsCollection.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Shipment.Code, true, false, Constants.TransportModes.Sea, Anything), () => AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingDomesticSeaOther.Value);
			DefaultDepartmentsCollection.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Shipment.Code, true, false, Constants.TransportModes.Air, Constants.ContainerModes.ULD), () => AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingDomesticAirULD.Value);
			DefaultDepartmentsCollection.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Shipment.Code, true, false, Constants.TransportModes.SeaAir, Anything), () => AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingDomesticAirOther.Value);
			DefaultDepartmentsCollection.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Shipment.Code, true, false, Constants.TransportModes.AirSea, Anything), () => AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingDomesticAirOther.Value);
			DefaultDepartmentsCollection.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Shipment.Code, true, false, Constants.TransportModes.Courier, Anything), () => AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingDomesticAirOther.Value);
			DefaultDepartmentsCollection.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Shipment.Code, true, false, Constants.TransportModes.Air, Anything), () => AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingDomesticAirOther.Value);
			DefaultDepartmentsCollection.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Shipment.Code, true, false, Constants.TransportModes.Rail, Anything), () => AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingDomesticRail.Value);
			DefaultDepartmentsCollection.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Shipment.Code, true, false, Constants.TransportModes.Road, Anything), () => AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingDomesticRoad.Value);

			DefaultDepartmentsCollection.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Shipment.Code, false, true, Constants.TransportModes.Sea, Constants.ContainerModes.FCL), () => AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingForeignSeaFcl.Value);
			DefaultDepartmentsCollection.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Shipment.Code, false, true, Constants.TransportModes.Sea, Constants.ContainerModes.LCL), () => AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingForeignSeaLcl.Value);
			DefaultDepartmentsCollection.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Shipment.Code, false, true, Constants.TransportModes.Sea, Anything), () => AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingForeignSeaOther.Value);
			DefaultDepartmentsCollection.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Shipment.Code, false, true, Constants.TransportModes.Air, Constants.ContainerModes.ULD), () => AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingForeignAirULD.Value);
			DefaultDepartmentsCollection.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Shipment.Code, false, true, Constants.TransportModes.SeaAir, Anything), () => AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingForeignAirOther.Value);
			DefaultDepartmentsCollection.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Shipment.Code, false, true, Constants.TransportModes.AirSea, Anything), () => AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingForeignAirOther.Value);
			DefaultDepartmentsCollection.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Shipment.Code, false, true, Constants.TransportModes.Courier, Anything), () => AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingForeignAirOther.Value);
			DefaultDepartmentsCollection.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Shipment.Code, false, true, Constants.TransportModes.Air, Anything), () => AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingForeignAirOther.Value);
			DefaultDepartmentsCollection.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Shipment.Code, false, true, Constants.TransportModes.Rail, Anything), () => AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingForeignRail.Value);
			DefaultDepartmentsCollection.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Shipment.Code, false, true, Constants.TransportModes.Road, Anything), () => AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingForeignRoad.Value);

			// DECLARATIONS
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Brokerage.Code, false, false, Constants.TransportModes.Sea, Constants.ContainerModes.FCL), () => AccountingConfigurationRegistry.Instance.CustomsImportSeaFcl.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Brokerage.Code, false, false, Constants.TransportModes.Sea, Constants.ContainerModes.LCL), () => AccountingConfigurationRegistry.Instance.CustomsImportSeaLcl.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Brokerage.Code, false, false, Constants.TransportModes.Sea, Anything), () => AccountingConfigurationRegistry.Instance.CustomsImportSeaFcl.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Brokerage.Code, false, false, Constants.TransportModes.Air, Anything), () => AccountingConfigurationRegistry.Instance.CustomsImportAirUld.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Brokerage.Code, false, false, Constants.TransportModes.Rail, Anything), () => AccountingConfigurationRegistry.Instance.CustomsImportRail.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Brokerage.Code, false, false, Constants.TransportModes.Road, Anything), () => AccountingConfigurationRegistry.Instance.CustomsImportRoad.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Brokerage.Code, false, false, Constants.TransportModes.Mail, Anything), () => AccountingConfigurationRegistry.Instance.CustomsImportPost.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Brokerage.Code, false, false, Post, Anything), () => AccountingConfigurationRegistry.Instance.CustomsImportPost.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Brokerage.Code, false, false, Constants.TransportModes.Other, Anything), () => AccountingConfigurationRegistry.Instance.CustomsImportOther.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));

			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Brokerage.Code, true, true, Constants.TransportModes.Sea, Constants.ContainerModes.FCL), () => AccountingConfigurationRegistry.Instance.CustomsExportSeaFcl.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Brokerage.Code, true, true, Constants.TransportModes.Sea, Constants.ContainerModes.LCL), () => AccountingConfigurationRegistry.Instance.CustomsExportSeaLcl.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Brokerage.Code, true, true, Constants.TransportModes.Sea, Anything), () => AccountingConfigurationRegistry.Instance.CustomsExportSeaFcl.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Brokerage.Code, true, true, Constants.TransportModes.Air, Anything), () => AccountingConfigurationRegistry.Instance.CustomsExportAirUld.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Brokerage.Code, true, true, Constants.TransportModes.Rail, Anything), () => AccountingConfigurationRegistry.Instance.CustomsExportRail.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Brokerage.Code, true, true, Constants.TransportModes.Road, Anything), () => AccountingConfigurationRegistry.Instance.CustomsExportRoad.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Brokerage.Code, true, true, Constants.TransportModes.Mail, Anything), () => AccountingConfigurationRegistry.Instance.CustomsExportPost.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Brokerage.Code, true, true, Post, Anything), () => AccountingConfigurationRegistry.Instance.CustomsExportPost.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Brokerage.Code, true, true, Constants.TransportModes.Other, Anything), () => AccountingConfigurationRegistry.Instance.CustomsOther.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));

			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Brokerage.Code, false, false, JobMessageTypeList.Codes.ExWarehouse, Anything), () => AccountingConfigurationRegistry.Instance.CustomsExWarehouse.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Brokerage.Code, false, false, JobMessageTypeList.Codes.MiscellaneousCustoms, Anything), () => AccountingConfigurationRegistry.Instance.CustomsOther.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Brokerage.Code, false, false, JobMessageTypeList.Codes.Drawback, Anything), () => AccountingConfigurationRegistry.Instance.CustomsOther.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Brokerage.Code, false, false, JobMessageTypeList.Codes.Refund, Anything), () => AccountingConfigurationRegistry.Instance.CustomsOther.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Brokerage.Code, false, false, Excise, Anything), () => AccountingConfigurationRegistry.Instance.CustomsOther.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.PostClearanceBrokerage.Code, false, false, Anything, Anything), () => AccountingConfigurationRegistry.Instance.CustomsOther.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));

			//NCTS
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CustomsTransitNCTS.Code, false, false, Constants.TransportModes.Sea, Constants.ContainerModes.FCL), () => AccountingConfigurationRegistry.Instance.NCTSDepartureSeaFcl.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CustomsTransitNCTS.Code, false, false, Constants.TransportModes.Sea, Constants.ContainerModes.LCL), () => AccountingConfigurationRegistry.Instance.NCTSDepartureSeaLcl.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CustomsTransitNCTS.Code, false, false, Constants.TransportModes.Road, Anything), () => AccountingConfigurationRegistry.Instance.NCTSDepartureRoad.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CustomsTransitNCTS.Code, false, false, Constants.TransportModes.Rail, Anything), () => AccountingConfigurationRegistry.Instance.NCTSDepartureRail.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CustomsTransitNCTS.Code, false, false, Constants.TransportModes.Air, Anything), () => AccountingConfigurationRegistry.Instance.NCTSDepartureAir.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CustomsTransitNCTS.Code, false, false, Constants.TransportModes.Mail, Anything), () => AccountingConfigurationRegistry.Instance.NCTSDeparturePost.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CustomsTransitNCTS.Code, false, false, Constants.TransportModes.Unknown, Anything), () => AccountingConfigurationRegistry.Instance.NCTSDepartureOther.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CustomsTransitNCTS.Code, false, false, Constants.TransportModes.Other, Anything), () => AccountingConfigurationRegistry.Instance.NCTSArrival.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));

			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CustomsTransitNCTS.Code, true, false, Constants.TransportModes.Sea, Constants.ContainerModes.FCL), () => AccountingConfigurationRegistry.Instance.NCTSDepartureSeaFcl.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CustomsTransitNCTS.Code, true, false, Constants.TransportModes.Sea, Constants.ContainerModes.LCL), () => AccountingConfigurationRegistry.Instance.NCTSDepartureSeaLcl.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CustomsTransitNCTS.Code, true, false, Constants.TransportModes.Road, Anything), () => AccountingConfigurationRegistry.Instance.NCTSDepartureRoad.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CustomsTransitNCTS.Code, true, false, Constants.TransportModes.Rail, Anything), () => AccountingConfigurationRegistry.Instance.NCTSDepartureRail.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CustomsTransitNCTS.Code, true, false, Constants.TransportModes.Air, Anything), () => AccountingConfigurationRegistry.Instance.NCTSDepartureAir.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CustomsTransitNCTS.Code, true, false, Constants.TransportModes.Mail, Anything), () => AccountingConfigurationRegistry.Instance.NCTSDeparturePost.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CustomsTransitNCTS.Code, true, false, Constants.TransportModes.Unknown, Anything), () => AccountingConfigurationRegistry.Instance.NCTSDepartureOther.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CustomsTransitNCTS.Code, true, false, Constants.TransportModes.Other, Anything), () => AccountingConfigurationRegistry.Instance.NCTSArrival.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));

			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CustomsTransitNCTS.Code, false, true, Constants.TransportModes.Sea, Constants.ContainerModes.FCL), () => AccountingConfigurationRegistry.Instance.NCTSDepartureSeaFcl.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CustomsTransitNCTS.Code, false, true, Constants.TransportModes.Sea, Constants.ContainerModes.LCL), () => AccountingConfigurationRegistry.Instance.NCTSDepartureSeaLcl.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CustomsTransitNCTS.Code, false, true, Constants.TransportModes.Road, Anything), () => AccountingConfigurationRegistry.Instance.NCTSDepartureRoad.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CustomsTransitNCTS.Code, false, true, Constants.TransportModes.Rail, Anything), () => AccountingConfigurationRegistry.Instance.NCTSDepartureRail.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CustomsTransitNCTS.Code, false, true, Constants.TransportModes.Air, Anything), () => AccountingConfigurationRegistry.Instance.NCTSDepartureAir.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CustomsTransitNCTS.Code, false, true, Constants.TransportModes.Mail, Anything), () => AccountingConfigurationRegistry.Instance.NCTSDeparturePost.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CustomsTransitNCTS.Code, false, true, Constants.TransportModes.Unknown, Anything), () => AccountingConfigurationRegistry.Instance.NCTSDepartureOther.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CustomsTransitNCTS.Code, false, true, Constants.TransportModes.Other, Anything), () => AccountingConfigurationRegistry.Instance.NCTSArrival.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));

			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CustomsTransitNCTS.Code, true, true, Constants.TransportModes.Sea, Constants.ContainerModes.FCL), () => AccountingConfigurationRegistry.Instance.NCTSDepartureSeaFcl.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CustomsTransitNCTS.Code, true, true, Constants.TransportModes.Sea, Constants.ContainerModes.LCL), () => AccountingConfigurationRegistry.Instance.NCTSDepartureSeaLcl.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CustomsTransitNCTS.Code, true, true, Constants.TransportModes.Road, Anything), () => AccountingConfigurationRegistry.Instance.NCTSDepartureRoad.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CustomsTransitNCTS.Code, true, true, Constants.TransportModes.Rail, Anything), () => AccountingConfigurationRegistry.Instance.NCTSDepartureRail.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CustomsTransitNCTS.Code, true, true, Constants.TransportModes.Air, Anything), () => AccountingConfigurationRegistry.Instance.NCTSDepartureAir.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CustomsTransitNCTS.Code, true, true, Constants.TransportModes.Mail, Anything), () => AccountingConfigurationRegistry.Instance.NCTSDeparturePost.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CustomsTransitNCTS.Code, true, true, Constants.TransportModes.Unknown, Anything), () => AccountingConfigurationRegistry.Instance.NCTSDepartureOther.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CustomsTransitNCTS.Code, true, true, Constants.TransportModes.Other, Anything), () => AccountingConfigurationRegistry.Instance.NCTSArrival.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));

			// CFS Shipment & Load List
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CFSShipment.Code, true, true, Constants.TransportModes.Air, Anything), () => AccountingConfigurationRegistry.Instance.CfsPackAir.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CFSShipment.Code, true, true, Constants.TransportModes.Sea, Anything), () => AccountingConfigurationRegistry.Instance.CfsPackSea.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CFSShipment.Code, true, true, Constants.TransportModes.Rail, Anything), () => AccountingConfigurationRegistry.Instance.CfsPackRail.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CFSShipment.Code, true, true, Constants.TransportModes.Road, Anything), () => AccountingConfigurationRegistry.Instance.CfsPackRoad.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));

			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CFSShipment.Code, false, false, Constants.TransportModes.Air, Anything), () => AccountingConfigurationRegistry.Instance.CfsUnpackAir.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CFSShipment.Code, false, false, Constants.TransportModes.Sea, Anything), () => AccountingConfigurationRegistry.Instance.CfsUnpackSea.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CFSShipment.Code, false, false, Constants.TransportModes.Rail, Anything), () => AccountingConfigurationRegistry.Instance.CfsUnpackRail.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CFSShipment.Code, false, false, Constants.TransportModes.Road, Anything), () => AccountingConfigurationRegistry.Instance.CfsUnpackRoad.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));

			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CFSLoadList.Code, true, true, Constants.TransportModes.Air, Anything), () => AccountingConfigurationRegistry.Instance.CfsPackAir.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CFSLoadList.Code, true, true, Constants.TransportModes.Sea, Anything), () => AccountingConfigurationRegistry.Instance.CfsPackSea.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CFSLoadList.Code, true, true, Constants.TransportModes.Rail, Anything), () => AccountingConfigurationRegistry.Instance.CfsPackRail.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CFSLoadList.Code, true, true, Constants.TransportModes.Road, Anything), () => AccountingConfigurationRegistry.Instance.CfsPackRoad.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));

			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CFSLoadList.Code, false, false, Constants.TransportModes.Air, Anything), () => AccountingConfigurationRegistry.Instance.CfsUnpackAir.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CFSLoadList.Code, false, false, Constants.TransportModes.Sea, Anything), () => AccountingConfigurationRegistry.Instance.CfsUnpackSea.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CFSLoadList.Code, false, false, Constants.TransportModes.Rail, Anything), () => AccountingConfigurationRegistry.Instance.CfsUnpackRail.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CFSLoadList.Code, false, false, Constants.TransportModes.Road, Anything), () => AccountingConfigurationRegistry.Instance.CfsUnpackRoad.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));

			// MasterAWB
			DefaultDepartmentsCollection.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.MasterAWB.Code, false, false, Anything, Anything), () => AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentMasterAWBDefaultDept.Value);

			//Warehouse
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.WarehouseInwards.Code, false, false, Anything, Anything), () => AccountingConfigurationRegistry.Instance.WarehouseDefaultDept.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.WarehouseOutwards.Code, false, false, Anything, Anything), () => AccountingConfigurationRegistry.Instance.WarehouseDefaultDept.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.WarehouseStorage.Code, false, false, Anything, Anything), () => AccountingConfigurationRegistry.Instance.WarehouseDefaultDept.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.WarehouseStocktake.Code, false, false, Anything, Anything), () => AccountingConfigurationRegistry.Instance.WarehouseDefaultDept.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.WarehouseAdHocServiceJob.Code, false, false, Anything, Anything), () => AccountingConfigurationRegistry.Instance.WarehouseDefaultDept.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.WarehouseVASOrder.Code, false, false, Anything, Anything), () => AccountingConfigurationRegistry.Instance.WarehouseDefaultDept.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));

			//Transit Warehouse
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.TransitReceive.Code, false, false, Anything, Anything), () => AccountingConfigurationRegistry.Instance.WarehouseDefaultDept.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.TransitReceiveTransportationUnit.Code, false, false, Anything, Anything), () => AccountingConfigurationRegistry.Instance.WarehouseDefaultDept.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.TransitDispatch.Code, false, false, Anything, Anything), () => AccountingConfigurationRegistry.Instance.WarehouseDefaultDept.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.TransitDispatchTransportationUnit.Code, false, false, Anything, Anything), () => AccountingConfigurationRegistry.Instance.WarehouseDefaultDept.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));

			// Process Management
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.WorkItem.Code, false, false, Anything, Anything), () => AccountingConfigurationRegistry.Instance.WorkitemDefaultDept.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.Project.Code, false, false, Anything, Anything), () => AccountingConfigurationRegistry.Instance.ProjectDefaultDept.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.WorkRequest.Code, false, false, Anything, Anything), () => AccountingConfigurationRegistry.Instance.CustomerServiceTicketDefaultDept.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));

			// CusMAWB
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CusMAWB.Code, false, false, Anything, Anything), () => AccountingConfigurationRegistry.Instance.AirCargo.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));

			// CusUnderbond
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CusUnderbond.Code, false, false, Anything, Anything), () => AccountingConfigurationRegistry.Instance.AirCargoOutturns.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));

			// CTOCusMAWB
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CTOCusMAWB.Code, false, false, Anything, Anything), () => AccountingConfigurationRegistry.Instance.AirCargoCTO.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));

			// CTOCusImportHAWB
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CTOCusImportHAWB.Code, false, false, Anything, Anything), () => AccountingConfigurationRegistry.Instance.AirCargoCTO.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));

			// CTOCusExportHAWB
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CTOCusExportHAWB.Code, false, false, Anything, Anything), () => AccountingConfigurationRegistry.Instance.AirCargoCTO.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			// Shipping Manager
			DepartmentCodes.Add(GetShippingDepartmentDictionaryKey(JobInvoicingConsumerTypes.AgencyBooking.Code, true, true, Constants.ContainerModes.FCL), () => AccountingConfigurationRegistry.Instance.ShippingExportContainerised.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetShippingDepartmentDictionaryKey(JobInvoicingConsumerTypes.AgencyBooking.Code, true, true, Constants.ContainerModes.BreakBulk), () => AccountingConfigurationRegistry.Instance.ShippingExportBreakBulk.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetShippingDepartmentDictionaryKey(JobInvoicingConsumerTypes.AgencyBooking.Code, true, true, Constants.ContainerModes.RollOnRollOff), () => AccountingConfigurationRegistry.Instance.ShippingExportRoRo.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetShippingDepartmentDictionaryKey(JobInvoicingConsumerTypes.AgencyBooking.Code, true, true, Constants.ContainerModes.Bulk), () => AccountingConfigurationRegistry.Instance.ShippingExportBulk.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetShippingDepartmentDictionaryKey(JobInvoicingConsumerTypes.AgencyBooking.Code, true, true, Constants.ContainerModes.Liquid), () => AccountingConfigurationRegistry.Instance.ShippingExportLiquid.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));

			DepartmentCodes.Add(GetShippingDepartmentDictionaryKey(JobInvoicingConsumerTypes.AgencyBooking.Code, false, false, Constants.ContainerModes.FCL), () => AccountingConfigurationRegistry.Instance.ShippingImportContainerised.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetShippingDepartmentDictionaryKey(JobInvoicingConsumerTypes.AgencyBooking.Code, false, false, Constants.ContainerModes.BreakBulk), () => AccountingConfigurationRegistry.Instance.ShippingImportBreakBulk.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetShippingDepartmentDictionaryKey(JobInvoicingConsumerTypes.AgencyBooking.Code, false, false, Constants.ContainerModes.RollOnRollOff), () => AccountingConfigurationRegistry.Instance.ShippingImportRoRo.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetShippingDepartmentDictionaryKey(JobInvoicingConsumerTypes.AgencyBooking.Code, false, false, Constants.ContainerModes.Bulk), () => AccountingConfigurationRegistry.Instance.ShippingImportBulk.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetShippingDepartmentDictionaryKey(JobInvoicingConsumerTypes.AgencyBooking.Code, false, false, Constants.ContainerModes.Liquid), () => AccountingConfigurationRegistry.Instance.ShippingImportLiquid.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));

			DepartmentCodes.Add(GetShippingDepartmentDictionaryKey(JobInvoicingConsumerTypes.AgencyBooking.Code, true, false, Constants.ContainerModes.FCL), () => AccountingConfigurationRegistry.Instance.ShippingDomesticContainerised.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetShippingDepartmentDictionaryKey(JobInvoicingConsumerTypes.AgencyBooking.Code, true, false, Constants.ContainerModes.BreakBulk), () => AccountingConfigurationRegistry.Instance.ShippingDomesticBreakBulk.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetShippingDepartmentDictionaryKey(JobInvoicingConsumerTypes.AgencyBooking.Code, true, false, Constants.ContainerModes.RollOnRollOff), () => AccountingConfigurationRegistry.Instance.ShippingDomesticRoRo.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetShippingDepartmentDictionaryKey(JobInvoicingConsumerTypes.AgencyBooking.Code, true, false, Constants.ContainerModes.Bulk), () => AccountingConfigurationRegistry.Instance.ShippingDomesticBulk.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetShippingDepartmentDictionaryKey(JobInvoicingConsumerTypes.AgencyBooking.Code, true, false, Constants.ContainerModes.Liquid), () => AccountingConfigurationRegistry.Instance.ShippingDomesticLiquid.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));

			DepartmentCodes.Add(GetShippingDepartmentDictionaryKey(JobInvoicingConsumerTypes.AgencyBooking.Code, false, true, Constants.ContainerModes.FCL), () => AccountingConfigurationRegistry.Instance.ShippingOtherContainerised.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetShippingDepartmentDictionaryKey(JobInvoicingConsumerTypes.AgencyBooking.Code, false, true, Constants.ContainerModes.BreakBulk), () => AccountingConfigurationRegistry.Instance.ShippingOtherBreakBulk.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetShippingDepartmentDictionaryKey(JobInvoicingConsumerTypes.AgencyBooking.Code, false, true, Constants.ContainerModes.RollOnRollOff), () => AccountingConfigurationRegistry.Instance.ShippingOtherRoRo.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetShippingDepartmentDictionaryKey(JobInvoicingConsumerTypes.AgencyBooking.Code, false, true, Constants.ContainerModes.Bulk), () => AccountingConfigurationRegistry.Instance.ShippingOtherBulk.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetShippingDepartmentDictionaryKey(JobInvoicingConsumerTypes.AgencyBooking.Code, false, true, Constants.ContainerModes.Liquid), () => AccountingConfigurationRegistry.Instance.ShippingOtherLiquid.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));

			DepartmentCodes.Add(GetShippingDepartmentDictionaryKey(JobInvoicingConsumerTypes.AgencyBillOfLading.Code, true, true, Constants.ContainerModes.FCL), () => AccountingConfigurationRegistry.Instance.ShippingExportContainerised.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetShippingDepartmentDictionaryKey(JobInvoicingConsumerTypes.AgencyBillOfLading.Code, true, true, Constants.ContainerModes.BreakBulk), () => AccountingConfigurationRegistry.Instance.ShippingExportBreakBulk.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetShippingDepartmentDictionaryKey(JobInvoicingConsumerTypes.AgencyBillOfLading.Code, true, true, Constants.ContainerModes.RollOnRollOff), () => AccountingConfigurationRegistry.Instance.ShippingExportRoRo.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetShippingDepartmentDictionaryKey(JobInvoicingConsumerTypes.AgencyBillOfLading.Code, true, true, Constants.ContainerModes.Bulk), () => AccountingConfigurationRegistry.Instance.ShippingExportBulk.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetShippingDepartmentDictionaryKey(JobInvoicingConsumerTypes.AgencyBillOfLading.Code, true, true, Constants.ContainerModes.Liquid), () => AccountingConfigurationRegistry.Instance.ShippingExportLiquid.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));

			DepartmentCodes.Add(GetShippingDepartmentDictionaryKey(JobInvoicingConsumerTypes.AgencyBillOfLading.Code, false, false, Constants.ContainerModes.FCL), () => AccountingConfigurationRegistry.Instance.ShippingImportContainerised.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetShippingDepartmentDictionaryKey(JobInvoicingConsumerTypes.AgencyBillOfLading.Code, false, false, Constants.ContainerModes.BreakBulk), () => AccountingConfigurationRegistry.Instance.ShippingImportBreakBulk.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetShippingDepartmentDictionaryKey(JobInvoicingConsumerTypes.AgencyBillOfLading.Code, false, false, Constants.ContainerModes.RollOnRollOff), () => AccountingConfigurationRegistry.Instance.ShippingImportRoRo.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetShippingDepartmentDictionaryKey(JobInvoicingConsumerTypes.AgencyBillOfLading.Code, false, false, Constants.ContainerModes.Bulk), () => AccountingConfigurationRegistry.Instance.ShippingImportBulk.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetShippingDepartmentDictionaryKey(JobInvoicingConsumerTypes.AgencyBillOfLading.Code, false, false, Constants.ContainerModes.Liquid), () => AccountingConfigurationRegistry.Instance.ShippingImportLiquid.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));

			DepartmentCodes.Add(GetShippingDepartmentDictionaryKey(JobInvoicingConsumerTypes.AgencyBillOfLading.Code, true, false, Constants.ContainerModes.FCL), () => AccountingConfigurationRegistry.Instance.ShippingDomesticContainerised.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetShippingDepartmentDictionaryKey(JobInvoicingConsumerTypes.AgencyBillOfLading.Code, true, false, Constants.ContainerModes.BreakBulk), () => AccountingConfigurationRegistry.Instance.ShippingDomesticBreakBulk.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetShippingDepartmentDictionaryKey(JobInvoicingConsumerTypes.AgencyBillOfLading.Code, true, false, Constants.ContainerModes.RollOnRollOff), () => AccountingConfigurationRegistry.Instance.ShippingDomesticRoRo.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetShippingDepartmentDictionaryKey(JobInvoicingConsumerTypes.AgencyBillOfLading.Code, true, false, Constants.ContainerModes.Bulk), () => AccountingConfigurationRegistry.Instance.ShippingDomesticBulk.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetShippingDepartmentDictionaryKey(JobInvoicingConsumerTypes.AgencyBillOfLading.Code, true, false, Constants.ContainerModes.Liquid), () => AccountingConfigurationRegistry.Instance.ShippingDomesticLiquid.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));

			DepartmentCodes.Add(GetShippingDepartmentDictionaryKey(JobInvoicingConsumerTypes.AgencyBillOfLading.Code, false, true, Constants.ContainerModes.FCL), () => AccountingConfigurationRegistry.Instance.ShippingOtherContainerised.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetShippingDepartmentDictionaryKey(JobInvoicingConsumerTypes.AgencyBillOfLading.Code, false, true, Constants.ContainerModes.BreakBulk), () => AccountingConfigurationRegistry.Instance.ShippingOtherBreakBulk.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetShippingDepartmentDictionaryKey(JobInvoicingConsumerTypes.AgencyBillOfLading.Code, false, true, Constants.ContainerModes.RollOnRollOff), () => AccountingConfigurationRegistry.Instance.ShippingOtherRoRo.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetShippingDepartmentDictionaryKey(JobInvoicingConsumerTypes.AgencyBillOfLading.Code, false, true, Constants.ContainerModes.Bulk), () => AccountingConfigurationRegistry.Instance.ShippingOtherBulk.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetShippingDepartmentDictionaryKey(JobInvoicingConsumerTypes.AgencyBillOfLading.Code, false, true, Constants.ContainerModes.Liquid), () => AccountingConfigurationRegistry.Instance.ShippingOtherLiquid.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));

			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.AgencyVoyageAccounting.Code, false, false, Anything, Anything), () => AccountingConfigurationRegistry.Instance.ShippingVoyageAccounting.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));

			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.AgencySundryCharges.Code, false, false, Anything, Anything), () => AccountingConfigurationRegistry.Instance.ShippingSundryCharges.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));

			// Importer Security Filing
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.ImporterSecurityFiling.Code, false, false, Anything, Anything), () => AccountingConfigurationRegistry.Instance.CustomsImportSeaFcl.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));

			// ContainerYard
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.FCLStorage.Code, false, false, Anything, Anything), () => AccountingConfigurationRegistry.Instance.ContainerYardJobsDefaultDept.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CYDReceiveAdvice.Code, false, false, Anything, Anything), () => AccountingConfigurationRegistry.Instance.ContainerYardJobsDefaultDept.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CYDReleaseAdvice.Code, false, false, Anything, Anything), () => AccountingConfigurationRegistry.Instance.ContainerYardJobsDefaultDept.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CYDTransportationUnit.Code, false, false, Anything, Anything), () => AccountingConfigurationRegistry.Instance.ContainerYardJobsDefaultDept.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CYDAdHocServiceOrder.Code, false, false, Anything, Anything), () => AccountingConfigurationRegistry.Instance.ContainerYardJobsDefaultDept.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.MNRWorkOrderHeader.Code, false, false, Anything, Anything), () => AccountingConfigurationRegistry.Instance.ContainerYardJobsDefaultDept.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.CYDPeriodicInvoicing.Code, false, false, Anything, Anything), () => AccountingConfigurationRegistry.Instance.ContainerYardJobsDefaultDept.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));

			//Transport Booking
			DepartmentCodes.Add(GetDepartmentDictionaryKey(JobInvoicingConsumerTypes.TransportBooking.Code, false, false, Anything, Anything), () => AccountingConfigurationRegistry.Instance.TransportBookingJobsDefaultDept.Value);
		}
		protected KeyForDictionary GetShippingDepartmentDictionaryKey(ZString tableName, bool isOriginLocal, bool isDestinationForeign, ZString containerMode)
		{
			var key = new KeyForDictionary();
			key.TableName = tableName;
			key.ConsolType = Constants.JobInvoicingDefaultDepartmentConsolType.NoConsol;
			key.IsOriginLocal = isOriginLocal;
			key.IsDestinationForeign = isDestinationForeign;
			key.ContainerMode = containerMode;
			return key;
		}

		protected KeyForDictionary GetDepartmentDictionaryKey(ZString tableName, bool isOriginLocal, bool isDestinationForeign, ZString transportMode, ZString containerMode)
		{
			return GetDepartmentDictionaryKey(tableName, Constants.JobInvoicingDefaultDepartmentConsolType.NoConsol, isOriginLocal, isDestinationForeign, transportMode, containerMode);
		}

		protected KeyForDictionary GetDepartmentDictionaryKey(ZString tableName, ZString consolType, bool isOriginLocal, bool isDestinationForeign, ZString transportMode, ZString containerMode)
		{
			ZString contMode = containerMode;

			switch (transportMode)
			{
				case Constants.TransportModes.Air:
					if (containerMode != Constants.ContainerModes.ULD)
					{
						contMode = Anything;
					}
					break;

				case Constants.TransportModes.Sea:
					if (containerMode != Constants.ContainerModes.FCL && containerMode != Constants.ContainerModes.LCL)
					{
						contMode = Anything;
					}
					break;

				case Constants.TransportModes.Rail:
					if ((containerMode != Constants.ContainerModes.FCL && containerMode != Constants.ContainerModes.LCL) ||
							(IsDomestic(isOriginLocal, isDestinationForeign) || IsForeign(isOriginLocal, isDestinationForeign)))
					{
						contMode = Anything;
					}
					break;

				default:
					contMode = Anything;
					break;
			}

			var key = new KeyForDictionary();
			key.TableName = tableName;
			key.ConsolType = consolType;
			key.IsOriginLocal = isOriginLocal;
			key.IsDestinationForeign = isDestinationForeign;
			key.TransportMode = transportMode;
			key.ContainerMode = contMode;

			return key;
		}

		bool IsForeign(bool isOriginLocal, bool isDestinationForeign)
		{
			return !isOriginLocal && isDestinationForeign;
		}

		bool IsDomestic(bool isOriginLocal, bool isDestinationForeign)
		{
			return isOriginLocal && !isDestinationForeign;
		}

		protected ZGuid GetDepartmentFromDictionary(KeyForDictionary key)
		{
			var department = ZGuid.Empty;
			if (key.TableName == JobInvoicingConsumerTypes.Shipment.Code || key.TableName == JobInvoicingConsumerTypes.MasterAWB.Code)
			{
				var consolType = key.ConsolType;
				if (key.ConsolType != Constants.JobInvoicingDefaultDepartmentConsolType.NoConsol)
				{
					key.ConsolType = Constants.JobInvoicingDefaultDepartmentConsolType.NoConsol;
				}
				Func<JobInvoicingDefaultDepartmentsCollection> getDepartmentCollection;
				if (DefaultDepartmentsCollection.TryGetValue(key, out getDepartmentCollection))
				{
					foreach (JobInvoicingDefaultDepartments departmentSetting in getDepartmentCollection().Normalized)
					{
						if (departmentSetting.ConsolType == consolType)
						{
							department = departmentSetting.Department;
							break;
						}
					}
				}
			}
			else if (key.TableName == JobInvoicingConsumerTypes.GatewayConsol.Code)
			{
				return GetRankedDepartment(key);
			}
			else
			{
				Func<ZGuid> getDepartmentCode;
				if (DepartmentCodes.TryGetValue(key, out getDepartmentCode))
				{
					department = getDepartmentCode();
				}
			}
			return department;
		}

		ZGuid GetRankedDepartment(KeyForDictionary key)
		{
			ZString boolsToDirection(bool isOriginLocal, bool isDestinationForeign)
			{
				return isOriginLocal
					? isDestinationForeign
						? Constants.FreightShipmentDirection.Code.Export
						: Constants.FreightShipmentDirection.Code.Domestic
					: isDestinationForeign
						? Constants.FreightShipmentDirection.Code.Other
						: Constants.FreightShipmentDirection.Code.Import;
			}

			var ranker = new StringColumnValueRanker();
			ranker.Add(nameof(JobInvoicingDefaultGatewayDepartments.Direction), new IZType[] { boolsToDirection(key.IsOriginLocal, key.IsDestinationForeign), (ZString)Constants.FreightShipmentDirection.Code.All });
			ranker.Add(nameof(JobInvoicingDefaultGatewayDepartments.TransportMode), new IZType[] { key.TransportMode, (ZString)Constants.TransportModes.All });
			ranker.Add(nameof(JobInvoicingDefaultGatewayDepartments.ConsolType), new IZType[] { key.ConsolType, (ZString)Constants.JobInvoicingDefaultDepartmentConsolType.All });

			var configs = AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentGateway.Value.Cast<JobInvoicingDefaultGatewayDepartments>().ToList();
			var matchedConfig = (ranker.GetBestMatch(configs) ?? Enumerable.Empty<JobInvoicingDefaultGatewayDepartments>()).FirstOrDefault();
			return matchedConfig?.Department ?? ZGuid.Empty;
		}

		protected virtual bool UseDefaultLoginDepartment(ZString consumerType)
		{
			bool result = false;

			if (!EnvProxy.Instance.CurrentUser.IsWebUser)
			{
				if (consumerType == JobInvoicingConsumerTypes.Shipment.Code
					|| consumerType == JobInvoicingConsumerTypes.OneOffQuotation.Code
					|| consumerType == JobInvoicingConsumerTypes.QuotedBooking.Code)
				{
					result = AccountingConfigurationRegistry.Instance.ForwardingDefaultToCurrentLoginDept.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty);
				}
				else if (consumerType == JobInvoicingConsumerTypes.GatewayConsol.Code)
				{
					result = AccountingConfigurationRegistry.Instance.GatewayDefaultToCurrentLoginDept.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty);
				}
				else if (consumerType == JobInvoicingConsumerTypes.Brokerage.Code || consumerType == JobInvoicingConsumerTypes.ImporterSecurityFiling.Code || consumerType == JobInvoicingConsumerTypes.PostClearanceBrokerage.Code || consumerType == JobInvoicingConsumerTypes.CustomsTemporaryStorage.Code)
				{
					result = AccountingConfigurationRegistry.Instance.CustomsDefaultToCurrentLoginDept.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty);
				}
				else if (consumerType == JobInvoicingConsumerTypes.CFSLoadList.Code || consumerType == JobInvoicingConsumerTypes.CFSShipment.Code)
				{
					result = AccountingConfigurationRegistry.Instance.CfsDefaultToCurrentLoginDept.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty);
				}
				else if (consumerType == JobInvoicingConsumerTypes.WarehouseInwards.Code || consumerType == JobInvoicingConsumerTypes.WarehouseOutwards.Code
						|| consumerType == JobInvoicingConsumerTypes.WarehouseStorage.Code || consumerType == JobInvoicingConsumerTypes.TransitReceive.Code
						|| consumerType == JobInvoicingConsumerTypes.TransitDispatch.Code || consumerType == JobInvoicingConsumerTypes.TransitReceiveTransportationUnit.Code
						|| consumerType == JobInvoicingConsumerTypes.TransitDispatchTransportationUnit.Code)
				{
					result = AccountingConfigurationRegistry.Instance.WarehouseDefaultToCurrentLoginDept.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty);
				}
				else if (consumerType == JobInvoicingConsumerTypes.WorkItem.Code)
				{
					result = AccountingConfigurationRegistry.Instance.WorkItemDefaultToCurrentLoginDept.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty);
				}
				else if (consumerType == JobInvoicingConsumerTypes.Project.Code)
				{
					result = AccountingConfigurationRegistry.Instance.ProjectDefaultToCurrentLoginDept.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty);
				}
				else if (consumerType == JobInvoicingConsumerTypes.WorkRequest.Code)
				{
					result = AccountingConfigurationRegistry.Instance.CustomerServiceTicketDefaultToCurrentLoginDept.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty);
				}
				else if (consumerType == JobInvoicingConsumerTypes.AgencyBooking.Code
					|| consumerType == JobInvoicingConsumerTypes.AgencyBillOfLading.Code
					|| consumerType == JobInvoicingConsumerTypes.AgencyDetentionInvoice.Code
					|| consumerType == JobInvoicingConsumerTypes.AgencyVoyageAccounting.Code
					|| consumerType == JobInvoicingConsumerTypes.AgencySundryCharges.Code)
				{
					result = AccountingConfigurationRegistry.Instance.ShippingDefaultToCurrentLoginDept.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty);
				}
				else if (consumerType == JobInvoicingConsumerTypes.TransportBooking.Code)
				{
					result = AccountingConfigurationRegistry.Instance.TransportBookingDefaultToCurrentLoginDept.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty);
				}
				else if (consumerType == JobInvoicingConsumerTypes.TransportConsignment.Code)
				{
					result = AccountingConfigurationRegistry.Instance.LandTransportDefaultToCurrentLoginDept.Value;
				}
			}

			return result;
		}

		protected virtual ZGuid GetDepartmentBasedOnCurrentLogin()
		{
			return GlbDepartment.CurrentDepartment.PK;
		}

#endregion
	}
}
