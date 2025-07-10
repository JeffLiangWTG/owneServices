using System;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public static class DocDataObjectExtensions
	{
		public static IDynamicData MakeDocDataDynamic(this DocDataObject docDataObject)
		{
			if (docDataObject == null)
			{
				return null;
			}

			var mateDataProvider = new DocDataObjectMetaDataProvider();
			var validationProvider = new DocDataObjectValidationProvider();
			var typeConverter = new DefaultTypeConverter();
			var dynamicDataFactory = new DocDataObjectDynamicDataFactory();

			return docDataObject.MakeDynamic(mateDataProvider, validationProvider, typeConverter, dynamicDataFactory);
		}

		/// <summary>
		///	Allows to specify which related properties needs to be validated when a property with property name changes value
		/// </summary>
		/// <param name="docDataObject">a DocDataObject</param>
		/// <param name="propertyName">Property which value we expect to change</param>
		/// <returns>OnValueChanged set up helper</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static IDocDataObjectOnValueChangedActionBuilder OnValueChanged(this DocDataObject docDataObject, string propertyName) =>
			docDataObject != null
				? new DocDataObjectOnValueChangedActionBuilder(docDataObject, propertyName)
				: throw new ArgumentNullException(nameof(docDataObject));
	}
}
