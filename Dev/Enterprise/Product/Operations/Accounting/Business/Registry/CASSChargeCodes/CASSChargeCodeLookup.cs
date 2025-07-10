using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ResString = Enterprise.Accounting.Business.ResString;

namespace Enterprise.Accounting.Registry.Business
{
	public class CASSChargeCodeLookups : ZLookups
	{
		public CASSChargeCodeLookups(CASSChargeCode parent)
			: base(parent)
		{
		}

		public CASSChargeCode CASSChargeCode
		{
			get { return (CASSChargeCode)Parent; }
		}

		public CodeDescriptionPairList CASSTypesList
		{
			get
			{
				return GetCASSTypes(CASSChargeCode.Factory);
			}
		}

		public CodeDescriptionPairList CASSLineComponentList
		{
			get
			{
				codeDescriptionPairList = new CodeDescriptionPairList();

				if (CASSChargeCode.CASSType == CASSTypes.Export.Code)
				{
					codeDescriptionPairList = CASSExportLineComponentList;
				}
				else if (CASSChargeCode.CASSType == CASSTypes.Import.Code)
				{
					codeDescriptionPairList = CASSImportLineComponentList;
				}

				if (!codeDescriptionPairList.ContainsCode(ALL))
				{
					codeDescriptionPairList.Insert(0, new CodeDescriptionPair(ALL, ZString.Empty));
				}

				return codeDescriptionPairList;
			}
		}
		CodeDescriptionPairList codeDescriptionPairList;

		public AccChargeCodeCollection ChargeCodeList
		{
			get
			{
				ZQuery filter = new ZQuery(AccChargeCodeSchema.AC_ChargeType, Constants.ChargeType.Disbursement);
				filter.AddToFilter(JoinCondition.Or, AccChargeCodeSchema.AC_ChargeType, SQLComparisonOperator.Equal, Constants.ChargeType.Margin);
				filter.AddToFilter(JoinCondition.Or, AccChargeCodeSchema.AC_ChargeType, SQLComparisonOperator.Equal, Constants.ChargeType.ManualJobAccrual);
				filter.AddToFilter(JoinCondition.And, AccChargeCodeSchema.AC_IsActive, true);
				var chargeCodeList = new AccChargeCodeCollection(CASSChargeCode.Factory, filter, CASSChargeCode.CASSChargeCodeCollection.CurrentFallbackLevel.CompanyPK(false));
				return chargeCodeList;
			}
		}

		#region Helper Functions

		public static CodeDescriptionPairList CASSExportLineComponentList
		{
			get
			{
				var cassExportLineComponentList = new CodeDescriptionPairList();
				cassExportLineComponentList.Add(CASSComponents.PrepaidWeightCharge);
				cassExportLineComponentList.Add(CASSComponents.PrepaidValuationCharge);
				cassExportLineComponentList.Add(CASSComponents.PrepaidChargesDueToTheCarrier);
				cassExportLineComponentList.Add(CASSComponents.CollectChargesDueToTheAgentExport);
				cassExportLineComponentList.Add(CASSComponents.Commission);
				cassExportLineComponentList.Add(CASSComponents.Discount);
				return cassExportLineComponentList;
			}
		}

		public static CodeDescriptionPairList CASSImportLineComponentList
		{
			get
			{
				var cassImportLineComponentList = new CodeDescriptionPairList();
				cassImportLineComponentList.Add(CASSComponents.WeightOrValuationCharge);
				cassImportLineComponentList.Add(CASSComponents.CollectChargesDueToTheAgentImport);
				cassImportLineComponentList.Add(CASSComponents.CollectChargesDueToTheCarrier);
				cassImportLineComponentList.Add(CASSComponents.StorageCharges);
				cassImportLineComponentList.Add(CASSComponents.CollectFee);
				cassImportLineComponentList.Add(CASSComponents.HandlingCharges);
				cassImportLineComponentList.Add(CASSComponents.OtherCharge1);
				cassImportLineComponentList.Add(CASSComponents.OtherCharge2);
				cassImportLineComponentList.Add(CASSComponents.MiscellaneousChargesAmount);
				return cassImportLineComponentList;
			}
		}

		static CodeDescriptionPairList GetCASSTypes(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(FindboxLookupCollections.CachingKey + "_CASSType", () => new CodeDescriptionPairList()
												{
													new CodeDescriptionPair(ALL, ZString.Empty),
													CASSTypes.Export,
													CASSTypes.Import,
												});
		}

		#endregion

		#region Helper Classes
		public static class CASSComponents
		{
			public static CodeDescriptionPair PrepaidWeightCharge { get { return new CodeDescriptionPair("PWC", ResString.GetMultilingualString("d2185be1-974c-41d7-b0f1-00e73f64f2a4", "Prepaid Weight Charge")); } }
			public static CodeDescriptionPair PrepaidValuationCharge { get { return new CodeDescriptionPair("PVC", ResString.GetMultilingualString("8de06475-2abc-4902-9bae-e2c2c8a1982a", "Prepaid Valuation Charge")); } }
			public static CodeDescriptionPair PrepaidChargesDueToTheCarrier { get { return new CodeDescriptionPair("PCC", ResString.GetMultilingualString("f5d2f82d-7823-4dad-a333-2e42f054b7c3", "Prepaid Charges due to the Carrier")); } }
			public static CodeDescriptionPair CollectChargesDueToTheAgentExport { get { return new CodeDescriptionPair("COA", ResString.GetMultilingualString("28e8940d-55f5-4daf-b8a2-79b67355adaf", "Collect Charges due to the Agent")); } }
			public static CodeDescriptionPair Commission { get { return new CodeDescriptionPair("COM", ResString.GetMultilingualString("b988de06-1143-4177-baae-3619fc775ed1", "Commission Charge")); } }
			public static CodeDescriptionPair Discount { get { return new CodeDescriptionPair("DOI", ResString.GetMultilingualString("33d50108-6835-4395-a5f0-bdbee22b4ffe", "Incentive Charge")); } }

			public static CodeDescriptionPair WeightOrValuationCharge { get { return new CodeDescriptionPair("WVC", ResString.GetMultilingualString("4e02f6dc-d8bf-496e-a03f-0bee8ece2cf5", "Weight/Valuation Charge")); } }
			public static CodeDescriptionPair CollectChargesDueToTheAgentImport { get { return new CodeDescriptionPair("CCA", ResString.GetMultilingualString("98eb22ee-31df-48f7-8fb6-3c9241fc9469", "Collect Charges due to Agent")); } }
			public static CodeDescriptionPair CollectChargesDueToTheCarrier { get { return new CodeDescriptionPair("CCC", ResString.GetMultilingualString("e239f4ff-8eae-405b-8316-d79e39e45a60", "Collect Charges due to the Carrier")); } }
			public static CodeDescriptionPair CollectFee { get { return new CodeDescriptionPair("COF", ResString.GetMultilingualString("dd8ed1f3-1017-432f-8457-f6a9892571b5", "Collect Fee")); } }
			public static CodeDescriptionPair HandlingCharges { get { return new CodeDescriptionPair("HDC", ResString.GetMultilingualString("a5364d64-0365-481a-b93d-3dc1f9e23fc8", "Handling Charges")); } }
			public static CodeDescriptionPair StorageCharges { get { return new CodeDescriptionPair("STC", ResString.GetMultilingualString("bf707abc-f93d-48ce-a6ee-6b4223d41716", "Storage Charges")); } }
			public static CodeDescriptionPair OtherCharge1 { get { return new CodeDescriptionPair("OC1", ResString.GetMultilingualString("cfea32a8-292b-4fef-b07c-d448fb9e32db", "Other Charge 1")); } }
			public static CodeDescriptionPair OtherCharge2 { get { return new CodeDescriptionPair("OC2", ResString.GetMultilingualString("0d79a4ad-706b-450f-ad42-e6e69a9bcdaf", "Other Charge 2")); } }
			public static CodeDescriptionPair MiscellaneousChargesAmount { get { return new CodeDescriptionPair("MCA", ResString.GetMultilingualString("bfe52fe3-10d5-4728-82ff-a6d2380e5462", "Miscellaneous Charges Amount")); } }
		}

		public static class CASSTypes
		{
			public static CodeDescriptionPair Export { get { return new CodeDescriptionPair("CEXP", ResString.GetMultilingualString("e8a7b7da-717d-44e5-b2af-91196b39740e", "CASS Export")); } }
			public static CodeDescriptionPair Import { get { return new CodeDescriptionPair("CIMP", ResString.GetMultilingualString("3d224911-5a25-4086-ac30-2f8f8cc8e5d4", "CASS Import")); } }
		}

		#endregion

		public const string ALL = "ALL";
	}
}
