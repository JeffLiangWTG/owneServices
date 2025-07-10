using System;
using System.Drawing;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	abstract class RegistryImageCollectionRegistryItemTest<TItem, TValue> : StronglyTypedRegistryItemTestCaseWithFactory<TValue>
			where TItem : RegistryImageCollectionRegistryItem<TValue>
			where TValue : RegistryImageCollection
	{
		public void TestLatestValue()
		{
			FallbackLevel fallback = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);

			using (Image image1 = new Bitmap(1, 1))
			using (Image image2 = new Bitmap(1, 2))
			{
				RegistryImage element1 = AddNew("cde", (NoResString)"dsc1", image1);
				Item.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Collection);

				AssertEquals("Value Count", 1, Item.Value.Count);
				AssertEquals("LatestValue should be empty if Fallback is null.", 0, Item.LatestValue(null).Count);
				AssertEquals("LatestValue Count", 1, Item.LatestValue(fallback).Count);

				RegistryImage testElement1 = Item.LatestValue(fallback)[0];
				AssertEquals("Code", element1.Code, testElement1.Code);
				AssertEquals("Description", element1.Description, testElement1.Description);
				Assert("Image should be equal", Utilities.IsImageEqual(element1.Image, testElement1.Image));

				RegistryImage element2 = AddNew("abc", (NoResString)"dsc2", image2);
				((IRegistryItemInternals)Item).SetProposedValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Collection);
				((IRegistryItemInternals)Item).SetCurrentValueToUse(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ValueToUse.ProposedValue);

				AssertEquals("Value Count", 1, Item.Value.Count);
				AssertEquals("LatestValue Count", 2, Item.LatestValue(fallback).Count);

				testElement1 = Item.LatestValue(fallback)[0];
				AssertEquals("Code", element1.Code, testElement1.Code);
				AssertEquals("Description", element1.Description, testElement1.Description);
				Assert("Image should be equal", Utilities.IsImageEqual(element1.Image, testElement1.Image));

				RegistryImage testElement2 = Item.LatestValue(fallback)[1];
				AssertEquals("Code", element2.Code, testElement2.Code);
				AssertEquals("Description", element2.Description, testElement2.Description);
				Assert("Image should be equal", Utilities.IsImageEqual(element2.Image, testElement2.Image));
			}
		}

		public virtual void TestLatestImagesCodeDescList()
		{
			FallbackLevel fallback = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);

			using (Image image1 = new Bitmap(1, 1))
			using (Image image2 = new Bitmap(1, 2))
			{
				RegistryImage element1 = AddNew("cde", (NoResString)"dsc", image1);
				Item.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Collection);

				AssertEquals("LatestImagesCodeDescList should be empty if Fallback is null.", 0, Item.LatestImagesCodeDescList(null).Count);
				CodeDescriptionPairList codeList = Item.LatestImagesCodeDescList(fallback);
				AssertEquals("CodeList Count", 1, codeList.Count);
				AssertEquals("Description", "dsc", codeList.GetDescriptionFromCode("cde"));

				RegistryImage element2 = AddNew("abc", (NoResString)"dsc2", image2);
				((IRegistryItemInternals)Item).SetProposedValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Collection);
				((IRegistryItemInternals)Item).SetCurrentValueToUse(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ValueToUse.ProposedValue);

				codeList = Item.LatestImagesCodeDescList(fallback);
				AssertEquals("CodeList Count", 2, codeList.Count);
				AssertEquals("Description", "dsc", codeList.GetDescriptionFromCode("cde"));
				AssertEquals("Description", "dsc2", codeList.GetDescriptionFromCode("abc"));
			}
		}

		public void TestSetAndDeleteValue()
		{
			TestCaseHelper.ClearTable(StmDataSchema.Constants.TableName);

			using (Image image1 = new Bitmap(1, 1))
			using (Image image2 = new Bitmap(1, 2))
			using (Image image3 = new Bitmap(1, 2))
			{
				RegistryImage element1 = AddNew("A", (NoResString)"a", image1);
				RegistryImage element2 = AddNew("B", (NoResString)"b", image2);
				Item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Collection);

				RegistryImage element3 = AddNew("C", (NoResString)"c", image3);
				Item.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Collection);

				element3.Image = image1;
				Item.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, Collection);

				Collection.RemoveAndDelete(element3);
				Item.SetValue(Env.CurrentCompany.PK, Guid.Empty, Env.CurrentDepartment.PK, Collection);

				RegistryImage element4 = AddNew("D", (NoResString)"d", image3);
				Item.SetValue(Env.CurrentDepartment.PK, Guid.Empty, Guid.Empty, Collection);

				Collection.RemoveAndDelete(element4);
				RegistryImage element5 = AddNew("D", (NoResString)"d", image3);
				Item.SetValue(Env.CurrentDepartment.PK, Guid.Empty, Guid.Empty, Collection);

				AssertEquals("Registry item count", 5, Factory.GetDatabaseCount(typeof(AutoStmData), new ZQuery(StmDataSchema.SD_Name, Item.Name)));
				AssertEquals("Image registry item count", 14, Factory.GetDatabaseCount(typeof(AutoStmData), new ZQuery(StmDataSchema.SD_Name, FreightDataRegistry.Instance.RegistryImageContainer.Name)));

				RegistryImageCollection systemValue = Item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				RegistryImageCollection companyValue = Item.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				RegistryImageCollection branchValue = Item.GetValueWithoutFallback(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty);
				RegistryImageCollection companyDepartmentValue = Item.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Env.CurrentDepartment.PK);
				RegistryImageCollection alternateCompanyValue = Item.GetValueWithoutFallback(Env.CurrentDepartment.PK, Guid.Empty, Guid.Empty);

				Values a = new Values("A", "a", image1);
				Values b = new Values("B", "b", image2);
				Values c3 = new Values("C", "c", image3);
				Values c1 = new Values("C", "c", image1);
				Values d = new Values("D", "d", image3);

				AssertCollection("systemValue", systemValue, GetFallbackKey(Guid.Empty, Guid.Empty, Guid.Empty), a, b);
				AssertCollection("companyValue", companyValue, GetFallbackKey(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), a, b, c3);
				AssertCollection("branchValue", branchValue, GetFallbackKey(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty), a, b, c1);
				AssertCollection("companyDepartmentValue", companyDepartmentValue, GetFallbackKey(Env.CurrentCompany.PK, Guid.Empty, Env.CurrentDepartment.PK), a, b);
				AssertCollection("alternateCompanyValue", alternateCompanyValue, GetFallbackKey(Env.CurrentDepartment.PK, Guid.Empty, Guid.Empty), a, b, d);

				DeleteValueAndAssert(alternateCompanyValue, Env.CurrentDepartment.PK, Guid.Empty, Guid.Empty, 5, 14);
				DeleteValueAndAssert(branchValue, Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, 5, 14);
				DeleteValueAndAssert(systemValue, Guid.Empty, Guid.Empty, Guid.Empty, 5, 14);
				DeleteValueAndAssert(companyDepartmentValue, Env.CurrentCompany.PK, Guid.Empty, Env.CurrentDepartment.PK, 5, 14);
				DeleteValueAndAssert(companyValue, Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, 5, 14);
			}
		}

		#region Implementation

		protected TValue Collection
		{
			get
			{
				if (collection == null)
				{
					collection = GetNewCollection();
				}
				return collection;
			}
		}

		void AssertCollection(string id, RegistryImageCollection collection, string expectedFallbackKey, params Values[] expectedElements)
		{
			AssertEquals(id + ".Count", expectedElements.Length, collection.Count);

			for (int i = 0; i < expectedElements.Length; i++)
			{
				string prefix = id + "[" + i + "].";

				AssertEquals(prefix + "Code", expectedElements[i].code, collection[i].Code);
				AssertEquals(prefix + "Description", expectedElements[i].description, collection[i].Description);
				AssertEquals(prefix + "fallbackKeyInDb", expectedFallbackKey, collection[i].FallbackKeyInDbForTest);

				using (collection[i].Image)
				{
					AssertEquals(prefix + "Image", true, Utilities.IsImageEqual(expectedElements[i].image, collection[i].Image));
				}
			}
		}

		void DeleteValueAndAssert(RegistryImageCollection collection, Guid companyPk, Guid branchPk, Guid departmentPk, int expectedRegistryItemCount, int expectedImageContainerCount)
		{
			IRegistryItemInternals imageContainerInternals = FreightDataRegistry.Instance.RegistryImageContainer;
			IRegistryItemInternals registryItemInternals = Item;

			foreach (RegistryImage element in collection)
			{
				AssertEquals("Image registry value should exist.", true, imageContainerInternals.HasActualValue(element.ImagePkForTest.ToGuid(), Guid.Empty, Guid.Empty));
			}

			registryItemInternals.DeleteValue(companyPk, branchPk, departmentPk);
			AssertEquals("Registry item count", expectedRegistryItemCount, Factory.GetDatabaseCount(typeof(AutoStmData), new ZQuery(StmDataSchema.SD_Name, Item.Name)));
			AssertEquals("Image registry item count", expectedImageContainerCount, Factory.GetDatabaseCount(typeof(AutoStmData), new ZQuery(StmDataSchema.SD_Name, FreightDataRegistry.Instance.RegistryImageContainer.Name)));

			foreach (RegistryImage element in collection)
			{
				AssertEquals("Image registry value should be deleted.", false, imageContainerInternals.HasActualValue(element.ImagePkForTest.ToGuid(), Guid.Empty, Guid.Empty));
			}
		}

		protected virtual RegistryImage AddNew(string code, MultilingualString description, Image image)
		{
			RegistryImage result = Collection.AddNew();
			result.Code = code;
			result.Description = description;
			result.Image = image;
			return result;
		}

		string GetFallbackKey(Guid companyPk, Guid branchPk, Guid departmentPk)
		{
			return new RegistryStorageKey(companyPk, branchPk, departmentPk).Key;
		}

		protected new TItem Item
		{
			get { return (TItem)base.Item; }
		}

		protected abstract TValue GetNewCollection();
		TValue collection;

		#region struct Values

		struct Values
		{
			public Values(string code, string description, Image image)
			{
				this.code = code;
				this.description = description;
				this.image = image;
			}

			public readonly string code;
			public readonly string description;
			public readonly Image image;
		}

		#endregion

		#endregion
	}
}
