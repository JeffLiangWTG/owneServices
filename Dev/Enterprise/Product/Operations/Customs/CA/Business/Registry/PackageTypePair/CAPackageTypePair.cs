using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Registry
{
	[XmlSerializerAssembly("Enterprise.Customs.CA.Business.XmlSerializers")]
	public class CAPackageTypePair : PackageTypePair
	{
		public override ZString CustomsPackageTypeFieldType => nameof(FieldType.Text);

		public override CodeDescriptionPairList CustomsPackageTypesList => Universal.RefCusCodeListTypes.GetCachedListValidBeforeDate(CurrentFactory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, UniversalReferenceConstants.UNPackTypeStartDate);
		protected override PackageTypePair GetInstanceForClone() => new CAPackageTypePair();

		protected override INotificationType InvalidCustomsPackageTypeNotificationType => NotificationType.Warning;

		public override IMultilingualString CustomsPackageTypeShouldBeInList
		{
			get { return Business.ResString.GetMultilingualString("d51dd965-642a-489b-85bf-d581221e7cab", "Customs Package Type should be in Customs Package Type list"); }
		}
	}
}
