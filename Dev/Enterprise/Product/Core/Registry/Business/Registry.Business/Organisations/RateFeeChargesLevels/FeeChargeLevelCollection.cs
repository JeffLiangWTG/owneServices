using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class FeeChargeLevelCollection : RegistryBusinessObjectCollectionTemplate
	{
		public static FeeChargeLevelCollection GetDefault()
		{
			var result = new FeeChargeLevelCollection();

			result.Add(new FeeChargeLevel
			{
				Code = ServiceLevelsConstants.Code.StandardLevel,
				Description = ServiceLevelsConstants.Description.StandardLevel,
				Amount1Type = OrgConstants.ServiceLevelAmountTypes.Code.None,
				Amount2Type = OrgConstants.ServiceLevelAmountTypes.Code.None
			});

			return result;
		}

		public new FeeChargeLevel this[int i]
		{
			get { return (FeeChargeLevel)base[i]; }
		}

		public new FeeChargeLevel AddNew()
		{
			return (FeeChargeLevel)base.AddNew();
		}

		protected sealed override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new FeeChargeLevelCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new FeeChargeLevel();
		}

		#region Constants

		static class ServiceLevelsConstants
		{
			internal static class Code
			{
				internal const string StandardLevel = "STD";
			}

			internal static class Description
			{
				internal static MultilingualString StandardLevel { get { return ResString.GetMultilingualString("6839160b-b51c-422b-b26c-ed9c9245e42f", "Standard Level"); } }
			}
		}
		#endregion
	}
}
