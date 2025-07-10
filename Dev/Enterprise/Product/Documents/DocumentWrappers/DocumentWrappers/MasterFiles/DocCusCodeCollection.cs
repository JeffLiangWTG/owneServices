using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocCusCodeCollection : DocumentWrapperCollection
	{
		public DocCusCodeCollection(OrgCusCodeCollection collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
			this.CollectionSource = collectionSource;
		}

		public DocCusCodeCollection(OrgAddressCusCodeCollection collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
			this.CollectionSource = collectionSource;
		}

		public ZString GetCustomsRegNoForCodeAndCountry(ZString codeType, RefCountry country)
		{
			var orgCusCodeCollection = (OrgCusCodeCollection)CollectionSource;
			if (orgCusCodeCollection != null)
			{
				return orgCusCodeCollection.GetCustomsRegNo(codeType, country);
			}
			var orgAddressCusCodeCollection = (OrgAddressCusCodeCollection)CollectionSource;
			if (orgAddressCusCodeCollection != null)
			{
				return orgAddressCusCodeCollection.GetCustomsRegNo(codeType, country.Code);
			}

			return ZString.Empty;
		}

		public ZString GetCustomsRegNoAndTypeForCodeAndCountry(ZString codeType, RefCountry country)
		{
			var result = GetCustomsRegNoForCodeAndCountry(codeType, country);

			if (!result.IsEmpty)
			{
				switch (codeType)
				{
					case BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ:
						return "CNPJ: " + result;
					default:
						return $"{codeType}: {result}";
				}
			}

			return ZString.Empty;
		}

		public new DocCusCode this[int index]
		{
			get { return (DocCusCode)base[index]; }
		}

		public new DocCusCode this[string index]
		{
			get
			{
				string[] parameters = index.Split(',');
				string codeType = parameters[0].Trim().ToUpper();
				string countryCode = (parameters.Length > 1) ? parameters[1].Trim().ToUpper() : "";
				foreach (DocCusCode cusCode in this)
				{
					if (cusCode.CodeType.ToUpper() == codeType && cusCode.Country != null && cusCode.Country.Code == countryCode)
					{
						return cusCode;
					}
				}
				return (DocCusCode)base[index];
			}
		}

		readonly IBusinessObjectCollection CollectionSource;
	}
}
