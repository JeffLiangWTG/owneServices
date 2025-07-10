using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class CusGoodsLocationAddress : EU.H7.Business.CusGoodsLocationAddress
	{
		public CusGoodsLocationAddress(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ResourceStringData("FED3CFB0-8C99-4C89-ABF2-488DD241CCF5", Caption = "Authorization Number", ShortCaption = "Authorization No.")]
		[List(nameof(Lookups) + "." + nameof(CusGoodsLocationAddressLookups.AuthorisationNumberList))]
		public override ZString AuthorisationNumber
		{
			get => base.AuthorisationNumber;
			set => base.AuthorisationNumber = value;
		}

		public new CusGoodsLocationAddressValidation Validation => (CusGoodsLocationAddressValidation)base.Validation;

		protected override JobDocAddressValidation GetNewValidation() => new CusGoodsLocationAddressValidation(this);

		public new CusGoodsLocation GoodsLocation => (CusGoodsLocation)base.GoodsLocation;

		TypeLoaderCollection parentLoaders;
		protected override TypeLoaderCollection ParentLoaders => parentLoaders ?? (parentLoaders = new TypeLoaderCollection(typeof(CusGoodsLocation)));

		protected override JobDocAddressLookups GetNewLookups() => new CusGoodsLocationAddressLookups(this);

		public new CusGoodsLocationAddressLookups Lookups => (CusGoodsLocationAddressLookups)base.Lookups;

		protected override bool AuthorisationNumberReadOnly => false;
	}
}
