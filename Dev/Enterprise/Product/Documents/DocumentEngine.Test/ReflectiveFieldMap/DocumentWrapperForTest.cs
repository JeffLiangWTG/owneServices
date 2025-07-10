using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.ReflectiveFieldMap.Testing
{
	public abstract class DocumentWrapperForTest : IBODocDataProvider
	{
		public abstract ZString ZStringIsIn { get; }
		public abstract ZString ZStringIsIn1 { get; }
		public abstract ZString ZStringIsIn2 { get; }
		public abstract ZString ZStringWithNoGetterIsOut { set; }
		public abstract ZString ZStringWithNoPublicGetterIsOut { protected get; set; }
		public abstract string StringIsOut { get; }
		public abstract bool BoolIsOut { get; }
		public abstract DocumentWrapperForTest Relation { get; }

		public abstract OrgHeader DocDataProviderIsIn { get; }
		public abstract OrgContact DocDataProviderIzIn { get; }
		public abstract object ObjectIsOut { get; }

		public abstract OrgAddressCollection DocDataProviderCollectionIsIn { get; }
		public abstract ZString[] CollectionIsIn { get; }
		public abstract IActiveBusinessObjectCollection IActiveBusinessObjectCollectionIsOut { get; }
		public abstract DocumentWrapperCollectionForTest Collection { get; }
		public DocumentWrapperCollectionWithCustomPropertiesForTest CollectionWithCustomProperties { get; }

		#region IBODocDataProvider Members

		DocWrapperCopyInfo IBODocDataProvider.AdditionalCopyInfo
		{
			get { throw new NotImplementedException(); }
		}

		CargoWise.EntityFramework.BusinessObject IBODocDataProvider.BusinessObjectToLogAgainst
		{
			get { throw new NotImplementedException(); }
		}

		ZString IBODocDataProvider.GetDocDataValue(ZString docDataIdentifier, ZString formatStringForFallbackValue)
		{
			throw new NotImplementedException();
		}

		string[] IBODocDataProvider.ImageNamesToRemove
		{
			get { throw new NotImplementedException(); }
		}

		CargoWise.EntityFramework.BusinessObject IBODocDataProvider.ParentBusinessObject
		{
			get { throw new NotImplementedException(); }
		}

		void IBODocDataProvider.SetDocWrapperContext(Dictionary<string, object> constants)
		{
			throw new NotImplementedException();
		}

		string IBODocDataProvider.ToString()
		{
			throw new NotImplementedException();
		}

		IZType IBODocDataProvider.GetCustomField(string fieldName, string typeName)
		{
			throw new NotImplementedException();
		}

		string IBODocDataProvider.GetCustomFieldCodeDescription(string fieldName, string typeName)
		{
			throw new NotImplementedException();
		}

		ZDateTime IBODocDataProvider.GetEventLastDateTime(string eventCode)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
