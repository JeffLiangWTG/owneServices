using System;
using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.Registry.Business.Testing
{
	abstract class RegistryImageCollectionTest<T> : RegistryBusinessObjectCollectionTestCase<T> where T : RegistryImageCollection
	{
		public void TestSetFallbackKey()
		{
			RegistryImage element1 = Collection.AddNew();
			RegistryImage element2 = Collection.AddNew();
			Collection.SetFallbackKey("!");
			AssertEquals("element1.FallbackKeyForSaving", "!", element1.FallbackKeyForSaving);
			AssertEquals("element2.FallbackKeyForSaving", "!", element2.FallbackKeyForSaving);
		}

		public void TestCleanUpDeletedElements()
		{
			IRegistryDataType dataType = new DummyNonPersistentBusinessObjectRegistryDataType(typeof(RegistryImage));

			RegistryImage element1 = Collection.AddNew();
			RegistryImage element2 = Collection.AddNew();

			using (Bitmap image = new Bitmap(1, 1))
			{
				element1.Image = image;
				element2.Image = image;

				Collection.SetFallbackKey("!");

				dataType.Serialise(element1);
				dataType.Serialise(element2);

				Collection.RemoveAndDelete(element1);
				Collection.CleanUpDeletedElements("!");

				IRegistryItemInternals itemInternals = FreightDataRegistry.Instance.RegistryImageContainer;

				AssertEquals("element1's image should be deleted.", false, itemInternals.HasActualValue(element1.ImagePkForTest.ToGuid(), Guid.Empty, Guid.Empty));
				AssertEquals("element2's image should not be deleted.", true, itemInternals.HasActualValue(element2.ImagePkForTest.ToGuid(), Guid.Empty, Guid.Empty));
			}
		}

		#region Implementation

		protected sealed override bool RequiresFactory
		{
			get { return false; }
		}

		protected sealed override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new RegistryImage
			{
				ImagePkForTest = ZGuid.NewZGuid(),
				Code = nextCode++.ToString()
			};
		}

		int nextCode;

		#endregion
	}
}
