using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.OverrideLevels;

namespace Enterprise.ZArchitecture.Environment.Registry.Testing
{
	sealed class RegistryItemSaveCsvHandlerTest : TestCaseWithFactory
	{
		public void TestSaveItems()
		{
			var list = CreateItems().ToList();
			using (TempFile document = TempFile.New())
			{
				using (var stream = new FileStream(document.Filename, FileMode.OpenOrCreate))
				{
					var saveHander = new RegistryItemSaveCsvHandler();
					saveHander.SaveItems(list, new DefaultOverrideLevel(), stream);
				}
				using (StreamReader reader = new StreamReader(document.Filename))
				{
					AssertEquals("CSV Header", "Category,Caption", reader.ReadLine());
					CombineAssertions("All Items should be correct", () =>
					{
						foreach (var item in list)
						{
							OCsvLine line = new OCsvLine(reader.ReadLine());
							AssertEquals(item.Category, line.FieldValues[0]);
							AssertEquals(item.Caption, line.FieldValues[1]);
						}
					}
					);
				}
			}
		}
		public IEnumerable<IRegistryItem> CreateItems()
		{
			var emptyString = (NoResString)"";
			var storage = RegistryStorageFlags.All;
			var name = GetRandomName();
			object defaultValue = null;
			yield return new IntRegistryItem(name, (NoResString)GetRandomName(), emptyString, storage, (int)(defaultValue ?? 0));
			yield return new StringRegistryItem(name, (NoResString)GetRandomName(), emptyString, emptyString, storage, (string)defaultValue);
			yield return new GuidRegistryItem(name, (NoResString)GetRandomName(), emptyString, emptyString, storage, (Guid)(defaultValue ?? Guid.Empty));
			yield return new DateTimeRegistryItem(name, (NoResString)GetRandomName(), emptyString, emptyString, storage, (DateTime)(defaultValue ?? ZDateTime.BrettsBirthday.ToDateTime()));
		}
		static string GetRandomName()
		{
			return Guid.NewGuid().ToString("N");
		}
	}
}
