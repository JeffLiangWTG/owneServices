using System;
using System.Collections;
using System.Globalization;
using CargoWise.Application;
using CargoWise.EntityFramework;

namespace Enterprise.BarcodeParsing.Business
{
	public static class BusinessObjectFactoryExtensions
	{
		/// <summary>
		/// This method loads the Barcode Parsing Consumer using the module code with ObjectFactory.
		/// </summary>
		/// <returns>Returns a matching or empty (default) Consumer object. Will not return null.</returns>
		public static IBarcodeParsingConsumer GetBarcodeParsingConsumerFromModuleCode(this BusinessObjectFactory factory, string moduleCode)
		{
			return factory.GetCachedValue($"BusinessObjectFactoryExtensions|GetBarcodeParsingConsumerFromModuleCode|{moduleCode}",
				() =>
				{
					IBarcodeParsingConsumer result = null;

					if (!string.IsNullOrEmpty(moduleCode) && BarcodeParsingLookupHelper.ModuleTypes(factory).ContainsCode(moduleCode))
					{
						var barcodeParsingConsumers = ObjectFactory.Get<Hashtable>("BarcodeParsingConsumers");
						if (barcodeParsingConsumers.ContainsKey(moduleCode))
						{
							var handle = (ObjectHandle)barcodeParsingConsumers[moduleCode];
							if (handle != null)
							{
								result = (IBarcodeParsingConsumer)handle.GetObject(new[] { factory });
							}
						}

						if (result == null)
						{
							throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
								$"All BarcodeModuleTypes must be mapped to BarcodeParsingConsumers in the application configuration list. Missing Mapping for BarcodeModuleType: {moduleCode}."));
						}
					}

					// in case user types an invalid module type (eg. "xXx"), we don't want to throw exceptions
					return result ?? new DefaultBarcodeParsingConsumer(factory);
				});
		}
	}
}
