using System;
using System.Collections;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Moq;

namespace Enterprise.BarcodeParsing.Business.Testing
{
	public class BarcodeParsingTestCase : TestCaseWithFactory
	{
		public static IDisposable EnableDummyBarcodeParsingConsumer(BusinessObjectFactory factory, IBarcodeParsingConsumer barcodeParsingConsumer = null)
		{
			const string BarcodeParsingConsumers = nameof(BarcodeParsingConsumers);

			IDisposable objectFactorySubstituteDisposable = null;

			return new DisposableAction(EnableDummyBarcodeParsingConsumer, DisableBarcodeParsingConsumer);

			void EnableDummyBarcodeParsingConsumer()
			{
				var consumers = ObjectFactory.Get<Hashtable>(BarcodeParsingConsumers);

				if (!consumers.ContainsKey(DummyBarcodeParsingConsumer.Module))
				{
					objectFactorySubstituteDisposable = SetupAndSubstituteConsumerMock();
				}

				WithSuspendListCache(moduleTypes => moduleTypes.AddPair(DummyBarcodeParsingConsumer.Module, "Dummy"));
				ClearDummyModuleInFactoryCache();

				IDisposable SetupAndSubstituteConsumerMock()
				{
					var objectHandleMock = new Mock<ObjectHandle>();

					objectHandleMock.Setup(x => x.GetObject(It.IsAny<object[]>())).Returns(barcodeParsingConsumer ?? new DummyBarcodeParsingConsumer(factory));

					var mockConsumersDictionary = new KeyObjectHandleDictionaryObject()
					{
						{ DummyBarcodeParsingConsumer.Module,  objectHandleMock.Object },
					};

					foreach (DictionaryEntry kvp in consumers)
					{
						mockConsumersDictionary.Add(kvp.Key, kvp.Value);
					}

					return ObjectFactory.Substitute(BarcodeParsingConsumers, mockConsumersDictionary);
				}
			}

			void DisableBarcodeParsingConsumer()
			{
				objectFactorySubstituteDisposable?.Dispose();
				WithSuspendListCache(moduleTypes => moduleTypes.RemoveCode(DummyBarcodeParsingConsumer.Module));
				ClearDummyModuleInFactoryCache();
			}

			void WithSuspendListCache(Action<BarcodeModuleTypes> action)
			{
				var moduleTypes = BarcodeParsingLookupHelper.ModuleTypes(factory);
				ICachedValueManager icachedValueManager = moduleTypes;
				icachedValueManager.IsCacheEnabled = false;

				using (new DisposableAction(() => icachedValueManager.IsCacheEnabled = true))
				{
					action(moduleTypes);
				}
			}

			void ClearDummyModuleInFactoryCache()
			{
				factory.ClearCachedValue<IBarcodeParsingConsumer>("BusinessObjectFactoryExtensions|GetBarcodeParsingConsumerFromModuleCode|DUM");
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			dummyBarcodeEnableDisposable = EnableDummyBarcodeParsingConsumer(Factory);
		}

		protected override void TearDown()
		{
			base.TearDown();
			dummyBarcodeEnableDisposable?.Dispose();
		}

		IDisposable dummyBarcodeEnableDisposable;

		protected BarcodeParsingTestHelper Helper
		{
			get { return helper ?? (helper = new BarcodeParsingTestHelper(Factory)); }
		}

		BarcodeParsingTestHelper helper;
	}
}
