using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class LicenceHeaderLookups : AutoLicenceHeaderLookups
	{
		public LicenceHeaderLookups(AutoLicenceHeader parent)
			: base(parent)
		{
		}

		#region AMS Mode List

		public AMS_USModes AMSModeList
		{
			get { return new AMS_USModes(); }
		}

		#endregion

		#region Support Modes

		public static class SupportModeConstants
		{
			public static class Codes
			{
				public const string NoSupport = "NOS";
				public const string Standard = "STD";
				public const string Hour24 = "24H";
			}

			public static class Descriptions
			{
				public const string NoSupport = "No Support";
				public const string Standard = "Standard Support";
				public const string Hour24 = "24-Hour Support";
			}
		}

		public CodeDescriptionPairList SupportModeList
		{
			get
			{
				CodeDescriptionPairList supportModeList = new CodeDescriptionPairList();

				supportModeList.AddPair(SupportModeConstants.Codes.NoSupport, SupportModeConstants.Descriptions.NoSupport);
				supportModeList.AddPair(SupportModeConstants.Codes.Standard, SupportModeConstants.Descriptions.Standard);
				supportModeList.AddPair(SupportModeConstants.Codes.Hour24, SupportModeConstants.Descriptions.Hour24);

				return supportModeList;
			}
		}

		#endregion

		#region Licence AdvStdOth

		public LicenceAdvStdOthList LicenceAdvStdOth
		{
			get { return new LicenceAdvStdOthList(); }
		}

		public ReadOnlyCodeDescriptionPairList ActiveEditions
		{
			get
			{
				var db = ((LicenceHeader)Parent).Database;
				var baseEdition = db?.BillingModel ?? "";
				return ActiveEditionsCache(Factory, baseEdition);
			}
		}

		public static ReadOnlyCodeDescriptionPairList ActiveEditionsCache(BusinessObjectFactory factory, string billingModel = "")
		{
			return factory.GetCachedValue<ReadOnlyCodeDescriptionPairList>("LicenceHeaderLookups.ActiveEditions." + billingModel, () =>
			{
				var result = new CodeDescriptionPairList();
				if (billingModel == BillingConstants.BillingModel.STL)
				{
					result.AddPair(LicenceAdvStdOthList.Codes.SeatTransaction, LicenceAdvStdOthList.Descriptions.SeatTransaction);
				}
				else
				{
					foreach (CodeDescriptionBool item in EDIDataRegistry.Instance.LicenceEditionActiveList.Value)
					{
						if (item.Bool)
						{
							result.Add(item);
						}
					}
				}
				return result;
			});
		}

		#endregion

		#region ProductTypeList

		public CodeDescriptionPairList ProductType
		{
			get
			{
				return new ProductTypes(true);
			}
		}

		#endregion
	}
}

