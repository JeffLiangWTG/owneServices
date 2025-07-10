using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentEngine.Testing
{
	public class DummyFromEmailDocDataProvider : NonPersistentBusinessObject, IBODocDataProvider
	{
		internal DummyFromEmailDocDataProvider()
		{
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
			if (fieldName.Equals(Report.CustomFieldEmailFromAddress, System.StringComparison.InvariantCultureIgnoreCase))
			{
				return (ZString)"CargoWise <PleaseDoNotReply@cargowise.com>";
			}
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
