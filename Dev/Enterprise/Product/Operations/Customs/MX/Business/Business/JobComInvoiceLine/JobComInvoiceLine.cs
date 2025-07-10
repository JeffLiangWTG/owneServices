using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common.MX;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.MX.Business
{
	public partial class JobComInvoiceLine : BaseJobComInvoiceLine, ICusSupportingInfoTypeSupporter
	{
		public JobComInvoiceLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new class Schema : AutoJobComInvoiceLine.Schema
		{
			public const string VehicleVIN = nameof(JobComInvoiceLine.VehicleVIN);
			public const string VehicleMileage = nameof(JobComInvoiceLine.VehicleMileage);
			public const string VehicleMileageUQ = nameof(JobComInvoiceLine.VehicleMileageUQ);
			public const string JI_Calc_Invoice = nameof(JobComInvoiceLine.JI_Calc_Invoice);
			public const string JI_Calc_MergedLineNumber = nameof(JobComInvoiceLine.JI_Calc_MergedLineNumber);
			public const string JI_Calc_OrderLineNumberAndSubLine = nameof(JobComInvoiceLine.JI_Calc_OrderLineNumberAndSubLine);
			public const string Observations = nameof(JobComInvoiceLine.Observations);
			public const string UnitPrice = nameof(JobComInvoiceLine.UnitPrice);
		}

		#endregion

		// GetTariffDescription - to be overridden once the Tariff is setup for a new country
		protected override ZString GetTariffDescription(ZString tariffCode) => "TARIFF_DESCRIPTION";
		protected override ZString CustomsCountryCodeCore => Core.Constants.CountryCodes.Mexico;
		protected override Type TypeOfPartUsedCore => typeof(OrgSupplierPart);

		public override ZGuid JI_CEI
		{
			get => base.JI_CEI;
			set
			{
				var oldValue = JI_CEI;
				base.JI_CEI = value;
				if (!IsCopying && oldValue != JI_CEI)
				{
					Declaration?.MarkAsNeedingValidation();
					InvoiceHeader?.MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.MX.Business.JobComInvoiceLine|JI_Calc_MergedLineNumber", Caption = "Merged Ln. #")]
		public override ZString JI_Calc_MergedLineNumber { get => base.JI_Calc_MergedLineNumber; }

		[ResourceStringData("Enterprise.Customs.MX.Business.JobComInvoiceLine|JI_RN_NKCountryOfExport", ShortCaption = "Destination", FullDescription = "The Country of Destination of the goods.")]
		public override ZString JI_RN_NKCountryOfExport
		{
			get => base.JI_RN_NKCountryOfExport;
			set => base.JI_RN_NKCountryOfExport = value;
		}

		#region Observations

		[ResourceStringData("Enterprise.Customs.MX.Business.JobComInvoiceLine|Observations", ShortCaption = "Observations", Caption = "Observations")]
		public ZString Observations
		{
			get => ObservationsNote.Text;
			set
			{
				ObservationsNote.SetNoteText(this, ObservationsInfo, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateObservations();
				}
				ObservationsInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ObservationsInfo
		{
			get { return GetZPropertyInfo(Schema.Observations); }
		}

		HiddenTextNote ObservationsNote => observationsNote ??= new HiddenTextNote(this, PredefinedNoteTypes.Instance.MXObservations.Description);
		HiddenTextNote observationsNote;

		#endregion

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new JobComInvoiceLineFetchStrategy(this);

		#region Permit

		[UniversalCopyCollectionEntity(CusSupportingInfoSchema.Constants.TableName, CusSupportingInfoSchema.Constants.CSI_ParentTableCode)]
		[ChildEditable(true)]
		public IdentifierCollection Identifiers
		{
			get
			{
				if (identifiers == null)
				{
					identifiers = new IdentifierCollection(this);
					identifiers.Load();
					RegisterEditableChildObject(identifiers);
				}
				return identifiers;
			}
		}

		IdentifierCollection identifiers;

		#endregion

		public IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			var result = new Dictionary<ZString, Type>
			{
				{ CusSupportingInfoTypeList.Codes.Identifier, typeof(Identifier) },
			};
			return result;
		}

		public IEnumerable<IBusinessObjectFetchStrategy> GetFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			if (!IsDeleted)
			{
				if (!IsExport)
				{
					JI_RN_NKCountryOfExport = ZString.Empty;
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.MX.Business.JobComInvoiceLine|VehicleVIN", ShortCaption = "VIN", Caption = "VIN", FullDescription = "The Vehicle Identification Number or Serial Number.")]
		[MaxLength(25)]
		public ZString VehicleVIN
		{
			get => FirstVehicle.CVH_SerialNumber;
			set
			{
				FirstVehicle.CVH_SerialNumber = value;
				VehicleVINInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo VehicleVINInfo => GetWrappedZPropertyInfo(Schema.VehicleVIN, x => FirstVehicle.CVH_SerialNumberInfo);

		[ResourceStringData("Enterprise.Customs.MX.Business.JobComInvoiceLine|VehicleMileage", ShortCaption = "Mileage", Caption = "Mileage", FullDescription = "The vehicle Mileage, stated in kilometers.")]
		[MaxLength(6)]
		public ZInt VehicleMileage
		{
			get => FirstVehicle.CVH_Mileage;
			set
			{
				FirstVehicle.CVH_Mileage = value;
				VehicleMileageInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo VehicleMileageInfo => GetWrappedZPropertyInfo(Schema.VehicleMileage, x => FirstVehicle.CVH_MileageInfo);

		[ResourceStringData("Enterprise.Customs.MX.Business.JobComInvoiceLine|VehicleMileageUQ", ShortCaption = "Mileage UQ", Caption = "Mileage UQ", FullDescription = "The vehicle Mileage UQ.")]
		[MaxLength(CusVehicle.Schema.CVH_MileageUQMaxLength)]
		[ReadOnly(true)]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.VehicleMileageUQList))]
		public ZString VehicleMileageUQ
		{
			get => FirstVehicle.CVH_MileageUQ;
			set
			{
				FirstVehicle.CVH_MileageUQ = value;
				VehicleMileageUQInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo VehicleMileageUQInfo => GetWrappedZPropertyInfo(Schema.VehicleMileageUQ, x => FirstVehicle.CVH_MileageUQInfo);

		protected override ICusVehicleCollection<Customs.Business.CusVehicle, BaseJobComInvoiceLine> GetNewCusVehicleCollection() => new CusVehicleCollection<CusVehicle, JobComInvoiceLine>(this);

		public new CusVehicle FirstVehicle => (CusVehicle)base.FirstVehicle;

		public override VehicleRelationshipType VehicleRelationship => VehicleRelationshipType.One;
	}
}
