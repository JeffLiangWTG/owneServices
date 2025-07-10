using System;
using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.BR.Business
{
	public interface IXmlLoadingObjectParent<T> : IBusiness where T : INonPersistentBusinessObjectCollection
	{
		T Collection { get; }
		Action<int, int, string> AddLog { get; set; }
		void LoadAndValidateXML(string fileName, Stream stream);
		bool CreateDataFromXml();
	}
	public abstract class XmlLoadingObjectParent<T, TCollection> : NonPersistentBusinessObject, IXmlLoadingObjectParent<TCollection> where TCollection : INonPersistentBusinessObjectCollection
	{
		public XmlLoadingObjectParent(JobDeclaration declaration) : base(declaration.Factory)
		{
			Declaration = Argument.NotNull(declaration, nameof(declaration));
		}

		public readonly JobDeclaration Declaration;

		protected bool hasValidResponse;

		public Action<int, int, string> AddLog { get; set; }

		public abstract TCollection Collection { get; }

		public void LoadAndValidateXML(string fileName, Stream stream)
		{
			Collection.RemoveAndDeleteAll();

			T responseData = ReadFile(stream);
			hasValidResponse = ValidateResponse(fileName, responseData);

			if (hasValidResponse)
			{
				LoadObjects(responseData);
			}
		}

		public abstract bool CreateDataFromXml();

		protected abstract bool ValidateResponse(string fileName, T responseData);

		protected abstract void LoadObjects(T responseData);

		protected abstract T ReadFile(Stream stream);
	}
}
