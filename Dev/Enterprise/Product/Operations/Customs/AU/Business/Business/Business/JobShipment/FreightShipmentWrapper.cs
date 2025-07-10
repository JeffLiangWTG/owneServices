using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	/// <summary>
	/// Summary description for FreightShipmentWrapper.
	/// </summary>
	public class FreightShipmentWrapper
	{
		public FreightShipmentWrapper(CommonShipment shipment, ForwardingConsol consol)
		{
			Shipment = shipment;
			Consol = consol;
		}

		public FreightShipmentWrapper(CommonShipment shipment, ForwardingConsol consol, IEManifestLine consignment)
			: this(shipment, consol)
		{
			Consignment = consignment;
		}

		public IEnumerable<JobDeclaration> GetJobDeclaration()
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(JobDeclarationSchema.JE_JS, Shipment.PK);
			foreach (Customs.Business.BaseJobDeclaration declaration in Shipment.Factory.Load(typeof(Customs.Business.BaseJobDeclaration), filter))
			{
				if (declaration.Branch != null && declaration.Branch.Company.GC_RN_NKCountryCode == "AU")
				{
					yield return declaration as JobDeclaration;
				}
			}
		}

		public AUCusEntryNumber GetExistingPermit()
		{
			ZQuery parentIDFilter = GetEntryNumberParentQuery();

			ZQuery sQLFilter = new ZQuery();
			sQLFilter.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			sQLFilter.AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.CustomsPermitClearanceNumber);

			ZQuery permitTypeFilter = new ZQuery(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Australia.ECN);
			permitTypeFilter.AddToFilter(JoinCondition.Or, CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, CusEntryNumberTypes.Australia.CRN);
			permitTypeFilter.AddToFilter(JoinCondition.Or, CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, CANType.CustomsAuthorityNumber.Code);
			permitTypeFilter.AddToFilter(JoinCondition.Or, CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, CANType.ContingencyCustomsAuthorityNumber.Code);
			permitTypeFilter.AddToFilter(JoinCondition.Or, CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, CusEntryNumber.EntryType.ImportManifestStatus);
			permitTypeFilter.AddToFilter(new ZQuery(CusEntryNumSchema.CE_EntryNum, SQLComparisonOperator.NotEqual, ""), JoinCondition.And);

			ZQuery exemptionCodeFilter = new ZQuery();
			exemptionCodeFilter.AddToFilter(JoinCondition.Or, CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, CusEntryNumberTypes.Australia.EX1);
			exemptionCodeFilter.AddToFilter(JoinCondition.Or, CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, CusEntryNumberTypes.Australia.EX2);
			exemptionCodeFilter.AddToFilter(JoinCondition.Or, CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, CusEntryNumberTypes.Australia.EX3);
			exemptionCodeFilter.AddToFilter(JoinCondition.Or, CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, CusEntryNumberTypes.Australia.EX5);
			exemptionCodeFilter.AddToFilter(JoinCondition.Or, CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, CusEntryNumberTypes.Australia.EX7);
			exemptionCodeFilter.AddToFilter(JoinCondition.Or, CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, CusEntryNumberTypes.Australia.EX9);
			exemptionCodeFilter.AddToFilter(JoinCondition.Or, CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, CusEntryNumberTypes.Australia.EXA);
			exemptionCodeFilter.AddToFilter(JoinCondition.Or, CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, CusEntryNumberTypes.Australia.EXB);
			exemptionCodeFilter.AddToFilter(JoinCondition.Or, CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, CusEntryNumberTypes.Australia.EXC);

			exemptionCodeFilter.AddToFilter(JoinCondition.Or, CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, new CANTypeList().GetAllCodes().Select(x => CMRExportExemptionCodes.Get3CharCode(x)));

			exemptionCodeFilter.AddToFilter(permitTypeFilter, JoinCondition.Or);

			sQLFilter.AddToFilter(parentIDFilter);
			sQLFilter.AddToFilter(exemptionCodeFilter);
			AUCusEntryNumber existingPermit = Shipment.Factory.LoadTop1<AUCusEntryNumber>(sQLFilter);
			return existingPermit;
		}

		public AUCusEntryNumber GetSpecificPermitType(ICollection<string> permitTypes)
		{
			ZQuery parentIDFilter = GetEntryNumberParentQuery();

			var filter = new ZQuery();
			filter.AddToFilter(parentIDFilter);
			filter.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			filter.AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.CustomsPermitClearanceNumber);
			filter.AddToFilter(CusEntryNumSchema.CE_EntryType, permitTypes);
			filter.AddToFilter(CusEntryNumSchema.CE_EntryNum, SQLComparisonOperator.NotEqual, string.Empty);

			var specificPermit = Shipment.Factory.LoadTop1<AUCusEntryNumber>(filter);
			return specificPermit;
		}

		ZQuery GetEntryNumberParentQuery()
		{
			ZQuery parentIDFilter = new ZQuery();
			ZQuery shipmentParentFilter = new ZQuery(CusEntryNumSchema.CE_ParentID, Shipment.PK);
			shipmentParentFilter.AddToFilter(CusEntryNumSchema.CE_ParentTable, CommonShipment.Schema.TableName);

			var linkedDeclarations = GetJobDeclaration();
			var linkedDeclarationsPks = linkedDeclarations.Select(x => x.PK);
			ZQuery declarationParentFilter = new ZQuery(CusEntryNumSchema.CE_ParentID, linkedDeclarationsPks);
			declarationParentFilter.AddToFilter(CusEntryNumSchema.CE_ParentTable, JobDeclaration.Schema.TableName);

			var linkedEntryHeaderPks = linkedDeclarations.SelectMany(declaration => declaration.ActiveEntryHeaders).Select(entryHeader => entryHeader.PK);
			ZQuery entryHeaderParentFilter = new ZQuery(CusEntryNumSchema.CE_ParentID, linkedEntryHeaderPks);
			entryHeaderParentFilter.AddToFilter(CusEntryNumSchema.CE_ParentTable, CusEntryHeader.Schema.TableName);

			parentIDFilter.AddToFilter(shipmentParentFilter, JoinCondition.Or);
			parentIDFilter.AddToFilter(declarationParentFilter, JoinCondition.Or);
			parentIDFilter.AddToFilter(entryHeaderParentFilter, JoinCondition.Or);

			return parentIDFilter;
		}

		#region Export Sub Manifest Information

		#region Preliminary line number handling

		public void CreatePreliminaryManifestLineNumber(int lineNumber)
		{
			var esmLineData = ESMPreliminaryLineSequence;
			if (esmLineData == null)
			{
				esmLineData = LineSequenceCollection.AddNew();
				esmLineData.CY_Code = CustomsManifestLineSequence.DataCodes.PreliminaryLineNumber;
				esmLineData.CY_Data = Consol != null ? Consol.JK_UniqueConsignRef : Shipment.JS_JK_ConsolID;
			}

			esmLineData.CY_Order = (ZShort)lineNumber;
		}

		public CustomsManifestLineSequence ESMPreliminaryLineSequence
		{
			get
			{
				CustomsManifestLineSequence result = null;
				foreach (CustomsManifestLineSequence lineSequence in LineSequenceCollection)
				{
					if (lineSequence.CY_Code == CustomsManifestLineSequence.DataCodes.PreliminaryLineNumber)
					{
						result = lineSequence;
						break;
					}
				}

				return result;
			}
		}

		public bool IsPreliminaryNumberType
		{
			get { return ESMPreliminaryLineSequence != null; }
		}

		public void UpdatePreliminaryLineNumberToManifestedLineNumber()
		{
			if (ESMPreliminaryLineSequence != null)
			{
				ESMPreliminaryLineSequence.CY_Code = CustomsManifestLineSequence.DataCodes.ManifestLineNumber;
			}
		}

		#endregion

		#region Preliminary Deleted line number handling

		public CustomsManifestLineSequence ESMPreliminaryDeletedLineSequence
		{
			get
			{
				CustomsManifestLineSequence result = null;
				foreach (CustomsManifestLineSequence lineSequence in LineSequenceCollection)
				{
					if (lineSequence.CY_Code == CustomsManifestLineSequence.DataCodes.PreliminaryDeletedLine)
					{
						result = lineSequence;
						break;
					}
				}

				return result;
			}
		}

		#endregion

		#region Deleted line number handling

		public CustomsManifestLineSequence ESMDeletedLineSequence
		{
			get
			{
				CustomsManifestLineSequence result = null;
				foreach (CustomsManifestLineSequence lineSequence in LineSequenceCollection)
				{
					if (lineSequence.CY_Code == CustomsManifestLineSequence.DataCodes.DeletedLineNumber)
					{
						result = lineSequence;
						break;
					}
				}

				return result;
			}
		}

		public bool IsDeletedNumberType
		{
			get { return ESMDeletedLineSequence != null; }
		}

		public void UpdateManifestedLineNumberToPreliminaryDeletedLine()
		{
			if (ESMMainfestLineSequence != null)
			{
				ESMMainfestLineSequence.CY_Code = CustomsManifestLineSequence.DataCodes.PreliminaryDeletedLine;
			}
		}

		#endregion

		public int ESMLineNumber
		{
			get
			{
				int result = 0;
				if (ESMMainfestLineSequence != null)
				{
					result = ESMMainfestLineSequence.CY_Order;
				}

				return result;
			}
			set
			{
				var esmManifestLineData = ESMMainfestLineSequence;
				if (esmManifestLineData == null)
				{
					esmManifestLineData = LineSequenceCollection.AddNew();
					esmManifestLineData.CY_Code = CustomsManifestLineSequence.DataCodes.ManifestLineNumber;
					esmManifestLineData.CY_Data = Consol != null ? Consol.JK_UniqueConsignRef : Shipment.JS_JK_ConsolID;
				}

				esmManifestLineData.CY_Order = (ZShort)value;
			}
		}

		public CustomsManifestLineSequence ShipmentManifestLineSequence
		{
			get
			{
				if (LineSequenceCollection.Count > 0)
				{
					return LineSequenceCollection[0];
				}
				else
				{
					return null;
				}
			}
		}

		public CustomsManifestLineSequence ESMMainfestLineSequence
		{
			get
			{
				CustomsManifestLineSequence result = null;
				foreach (CustomsManifestLineSequence lineSequence in LineSequenceCollection)
				{
					if (lineSequence.CY_Code == CustomsManifestLineSequence.DataCodes.ManifestLineNumber)
					{
						if (Consol != null)
						{
							if (lineSequence.CY_Data == Consol.JK_UniqueConsignRef)
							{
								result = lineSequence;
								break;
							}
						}
						else
						{
							result = lineSequence;
							break;
						}
					}
				}

				return result;
			}
		}

		public bool HasSubManifestLineNumber
		{
			get { return ESMMainfestLineSequence != null & ESMLineNumber > 0; }
		}

		[ChildEditable(true)]
		public CustomsManifestLineSequenceCollection LineSequenceCollection
		{
			get
			{
				if (esmLineSequenceData == null)
				{
					if (Consignment is IHVLVConsignment hvlvConsignment)
					{
						esmLineSequenceData = new CustomsManifestLineSequenceCollection(hvlvConsignment);
						esmLineSequenceData.Load();
						(hvlvConsignment as BusinessObject)?.RegisterEditableChildObject(esmLineSequenceData);
					}
					else
					{
						esmLineSequenceData = new CustomsManifestLineSequenceCollection(Shipment);
						esmLineSequenceData.Load();
						Shipment.RegisterEditableChildObject(esmLineSequenceData);
					}
				}
				return esmLineSequenceData;
			}
		}
		CustomsManifestLineSequenceCollection esmLineSequenceData;

		#endregion

		public CommonShipment Shipment { get; set; }
		public ForwardingConsol Consol { get; set; }
		public IEManifestLine Consignment { get; set; }
	}
}
