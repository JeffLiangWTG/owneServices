using CargoWise.Application;
using CargoWise.Types;
using Moq;
using NUnit.Framework;
using static Enterprise.Integration.Customs.Shared;

namespace Enterprise.Customs.Common.Testing
{
	class EntryStatusListHelperTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestGetEntryStatusListProvider()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(EntryStatusListHelper.GetEntryStatusListProvider(Core.Constants.CountryCodes.Australia), Is.TypeOf(ObjectFactory.GetType("AU.IEntryStatusListProvider")));
				NUnit.Framework.Assert.That(EntryStatusListHelper.GetEntryStatusListProvider(Core.Constants.CountryCodes.Canada), Is.TypeOf(ObjectFactory.GetType("CA.IEntryStatusListProvider")));
				NUnit.Framework.Assert.That(EntryStatusListHelper.GetEntryStatusListProvider(Core.Constants.CountryCodes.UnitedArabEmirates), Is.TypeOf(ObjectFactory.GetType("AE.IEntryStatusListProvider")));
				NUnit.Framework.Assert.That(EntryStatusListHelper.GetEntryStatusListProvider(Core.Constants.CountryCodes.China), Is.TypeOf(ObjectFactory.GetType("CN.IEntryStatusListProvider")));
				NUnit.Framework.Assert.That(EntryStatusListHelper.GetEntryStatusListProvider(Core.Constants.CountryCodes.France), Is.TypeOf(ObjectFactory.GetType("FR.IEntryStatusListProvider")));
				NUnit.Framework.Assert.That(EntryStatusListHelper.GetEntryStatusListProvider(Core.Constants.CountryCodes.Singapore), Is.TypeOf(ObjectFactory.GetType("SG.IEntryStatusListProvider")));
				NUnit.Framework.Assert.That(EntryStatusListHelper.GetEntryStatusListProvider(Core.Constants.CountryCodes.Taiwan), Is.TypeOf(ObjectFactory.GetType("TW.IEntryStatusListProvider")));
				NUnit.Framework.Assert.That(EntryStatusListHelper.GetEntryStatusListProvider(Core.Constants.CountryCodes.NewZealand), Is.TypeOf(ObjectFactory.GetType("NZ.IEntryStatusListProvider")));
				NUnit.Framework.Assert.That(EntryStatusListHelper.GetEntryStatusListProvider(Core.Constants.CountryCodes.Germany), Is.TypeOf(ObjectFactory.GetType("DE.IEntryStatusListProvider")));
				NUnit.Framework.Assert.That(EntryStatusListHelper.GetEntryStatusListProvider(Core.Constants.CountryCodes.UnitedKingdom), Is.TypeOf(ObjectFactory.GetType("GB.IEntryStatusListProvider")));
				NUnit.Framework.Assert.That(EntryStatusListHelper.GetEntryStatusListProvider(Core.Constants.CountryCodes.UnitedStates), Is.TypeOf(ObjectFactory.GetType("US.IEntryStatusListProvider")));
				NUnit.Framework.Assert.That(EntryStatusListHelper.GetEntryStatusListProvider(Core.Constants.CountryCodes.Poland), Is.TypeOf(ObjectFactory.GetType("PL.IEntryStatusListProvider")));
				NUnit.Framework.Assert.That(EntryStatusListHelper.GetEntryStatusListProvider(Core.Constants.CountryCodes.HongKong), Is.TypeOf(ObjectFactory.GetType("Shared.IEntryStatusListProvider")));
				NUnit.Framework.Assert.That(EntryStatusListHelper.GetEntryStatusListProvider(Core.Constants.CountryCodes.Thailand), Is.TypeOf(ObjectFactory.GetType("Shared.IEntryStatusListProvider")));
				NUnit.Framework.Assert.That(EntryStatusListHelper.GetEntryStatusListProvider(Core.Constants.CountryCodes.Netherlands), Is.TypeOf(ObjectFactory.GetType("NL.IEntryStatusListProvider")));
			});

			var providerMock = new Mock<IAsycudaCustomsCountryProvider>();
			providerMock
				.Setup(m => m.IsAsycudaCustomsCountry(It.IsAny<ZString>()))
				.Returns((ZString zString) => Equals(zString, Core.Constants.CountryCodes.Congo));
			using (ObjectFactory.Substitute(providerMock.Object))
			{
				NUnit.Framework.Assert.That(EntryStatusListHelper.GetEntryStatusListProvider(Core.Constants.CountryCodes.Congo), Is.TypeOf(ObjectFactory.GetType("Asycuda.IEntryStatusListProvider")));
			}
			providerMock.VerifyAll();
		}
	}
}
