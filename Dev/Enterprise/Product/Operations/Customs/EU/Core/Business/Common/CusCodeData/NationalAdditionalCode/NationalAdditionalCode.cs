using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.Business
{
	public class NationalAdditionalCode : CusCodeDataWithOrder
	{
		public NationalAdditionalCode(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public static string CodeDataType => CusCodeDataTypeList.Codes.NationalAdditionalCode;

		public new class Loader : CusCodeDataWithOrder.Loader
		{
			public Loader(BusinessObjectFactory factory) : base(factory)
			{
			}

			public NationalAdditionalCode Load<TParent>(TParent parent, ZShort order)
				where TParent : BusinessObject
			{
				return Load<NationalAdditionalCode, TParent>(parent, order, CodeDataType);
			}

			public NationalAdditionalCode LoadOrCreate<TParent>(TParent parent, short order)
				where TParent : BusinessObject
			{
				return LoadOrCreate<NationalAdditionalCode, TParent>(parent, order, CodeDataType);
			}

			protected override Type GetTypeOfBusinessObjectToLoad() => typeof(NationalAdditionalCode);
		}

		public override bool SupportsNotes => false;

		protected override string CusCodeDataType => CodeDataType;

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(JobComInvoiceLine));
	}
}
