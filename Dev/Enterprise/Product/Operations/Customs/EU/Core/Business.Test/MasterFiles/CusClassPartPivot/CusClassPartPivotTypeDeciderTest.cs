using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Integration;
using Enterprise.Customs.EU.Business.MasterFiles;
using Moq;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	sealed class CusClassPartPivotTypeDeciderTest : Customs.Business.Testing.BaseCusClassPartPivotTypeDeciderTest
	{
		public void TestGetTypeForNew_ITypeDeciderContext()
		{
			var cusClassPartPivot = Factory.New<CusClassPartPivot>();
			AssertEquals("Enterprise.Customs.EU.Business.MasterFiles.CusClassPartPivot", cusClassPartPivot.GetType().FullName);

			var typeDeciderContextMock = new Mock<ITypeDeciderContext>();
			typeDeciderContextMock.Setup(k => k.Country).Returns("DE");
			cusClassPartPivot = Factory.New<CusClassPartPivot>(typeDeciderContextMock.Object);
			AssertEquals("Enterprise.Customs.DE.Business.MasterFiles.CusClassPartPivot", cusClassPartPivot.GetType().FullName);
		}

		protected override Type BaseTypeDecidedType => typeof(CusClassPartPivot);

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding()
		{
			var baseTypeDecidedType = BaseTypeDecidedType;
			var provider = ObjectFactory.Get<Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>();
			return base.GetTestCountryCodesAndExpectedTypesForBinding().ToDictionary(x => x.Key, y => provider.IsInEuropeanCustomsUnionOrInheritsFromEU(y.Key) ? y.Value : baseTypeDecidedType);
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew()
		{
			var baseTypeDecidedType = BaseTypeDecidedType;
			var provider = ObjectFactory.Get<Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>();
			return base.GetTestCountryCodesAndExpectedTypesForNew().ToDictionary(x => x.Key, y => provider.IsInEuropeanCustomsUnionOrInheritsFromEU(y.Key) ? y.Value : baseTypeDecidedType);
		}
	}
}
