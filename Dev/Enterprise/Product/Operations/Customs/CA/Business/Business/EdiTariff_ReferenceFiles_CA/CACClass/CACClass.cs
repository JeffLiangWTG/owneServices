using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	[DescriptionProperty(CACClassSchema.Constants.CT_LongDescription), CodeProperty(CACClassSchema.Constants.CT_Tariff)]
	public class CACClass : AutoCACClass
	{
		public CACClass(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCACClass.Schema
		{
			public const string CT_FormattedCode = "CT_FormattedCode";
		}

		public ZString CT_FormattedCode
		{
			get { return TariffFormatter.DisplayFormat(CT_Tariff); }
		}

		public ZPropertyInfo CT_FormattedCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CT_FormattedCode, x => CT_TariffInfo); }
		}

		protected TariffFormatter TariffFormatter
		{
			get { return new TariffFormatter(); }
		}

		#region Loader

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public CACClass LoadFromCode(ZString tariffCode)
			{
				try
				{
					return Factory.LoadFromNaturalKey<CACClass>(CACClassSchema.CT_Tariff, tariffCode);
				}
				catch (SqlException)
				{
					return null;
				}
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(CACClass);
			}
		}

		#endregion
	}
}
