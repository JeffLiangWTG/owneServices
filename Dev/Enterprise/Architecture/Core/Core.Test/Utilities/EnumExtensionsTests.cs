using System;
using CargoWiseOne.ResourceStrings;
using CargoWiseOne.ResourceStrings.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class EnumExtensionsTests : TestCase
	{
		public void TestGetCaption()
		{
			const string englishCaption = "EnglishCaption";
			const string germanCaption = "GermanCaption";
			const string key = "Key";

			using (IMockResourceStringCache grmMockData = Res.GetLanguageInstance(Enterprise.Core.SharedConstants.Languages.German).UseMockData())
			{
				grmMockData.Put(key, new ResourceStringData(key, germanCaption));
				AssertEquals(englishCaption, DummyEnum.EnumField1.GetCaption());

				using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.German))
				{
					AssertEquals(germanCaption, DummyEnum.EnumField1.GetCaption());
				}
				AssertEquals(englishCaption, DummyEnum.EnumField1.GetCaption());
				AssertExceptionThrown<ArgumentNullException>(() => DummyEnum.EnumField2.GetCaption());
			}
		}

		enum DummyEnum
		{
			[ResourceStringData("Key", Caption = "EnglishCaption")]
			EnumField1,
			EnumField2
		}
	}
}
