using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.CN.Business
{
	public partial class CusContainer : Customs.Business.BaseCusContainer
	{
		public CusContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema
		public new partial class Schema : Customs.Business.BaseCusContainer.Schema
		{
			public const string ContainerCode = "ContainerCode";
			public const string ContainerCodeDescription = "ContainerCodeDescription";
		}
		#endregion

		[ResourceStringData("Enterprise.Customs.CN.Business.RelatedContainer|ContainerCode", Caption = "CN Container Code")]
		public ZString ContainerCode => Container?.GetCountrySpecificContainerCode(Core.Constants.CountryCodes.China) ?? ZString.Empty;

		public ZPropertyInfo ContainerCodeInfo => GetZPropertyInfo(Schema.ContainerCode);

		[ResourceStringData("Enterprise.Customs.CN.Business.RelatedContainer|ContainerCodeDescription", Caption = "CN Container Code Description", ShortCaption = "CN Container Code Desc.")]
		public ZString ContainerCodeDescription
		{
			get
			{
				var list = Factory.GetCachedValue<CNContainerCodeList>();
				return list.GetDescriptionFromCode(ContainerCode);
			}
		}

		public ZPropertyInfo ContainerCodeDescriptionInfo => GetZPropertyInfo(Schema.ContainerCodeDescription);
	}
}
