using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class CompanyCountryCodeDocDataProvider : IBODocDataProvider
	{
		readonly ZString companyCountryCode;

		public CompanyCountryCodeDocDataProvider(ZString companyCountryCode)
		{
			this.companyCountryCode = companyCountryCode;
		}

		public ZString CompanyCountryCode
		{
			get { return companyCountryCode; }
		}

		#region IBODocDataProvider Members

		DocWrapperCopyInfo IBODocDataProvider.AdditionalCopyInfo
		{
			get { return null; }
		}

		CargoWise.EntityFramework.BusinessObject IBODocDataProvider.BusinessObjectToLogAgainst
		{
			get { return null; }
		}

		ZString IBODocDataProvider.GetDocDataValue(ZString docDataIdentifier, ZString formatStringForFallbackValue)
		{
			return ZString.Empty;
		}

		string[] IBODocDataProvider.ImageNamesToRemove
		{
			get { return null; }
		}

		CargoWise.EntityFramework.BusinessObject IBODocDataProvider.ParentBusinessObject
		{
			get { return null; }
		}

		void IBODocDataProvider.SetDocWrapperContext(Dictionary<string, object> constants)
		{
		}

		IZType IBODocDataProvider.GetCustomField(string fieldName, string typeName)
		{
			return ZString.Empty;
		}

		string IBODocDataProvider.GetCustomFieldCodeDescription(string fieldName, string typeName)
		{
			return string.Empty;
		}

		ZDateTime IBODocDataProvider.GetEventLastDateTime(string eventCode)
		{
			return ZDateTime.Empty;
		}

		#endregion
	}
}
