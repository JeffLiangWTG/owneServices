using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs.Testing
{
	[TestedType(typeof(LocationOfGoodsTypeConverter))]
	class LocationOfGoodsTypeConverterTest : EnumConverterTestCase<LocationOfGoodsTypeConverter, LocationOfGoodsType>
	{
		protected override IEnumerable<ZString> GetAllPossibleEnterpriseValues()
		{
			Type t = Type.GetType("Enterprise.Customs.Business.CusGoodsLocationUseList, Enterprise.Customs.Business, Version=2.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350");

			CodeDescriptionPairList codeValuePairList = (CodeDescriptionPairList)Activator.CreateInstance(t);

			IList<ZString> values = new List<ZString>();
			foreach (ZArchitecture.Core.CodeDescriptionPair item in codeValuePairList)
			{
				values.Add(item.Code);
			}

			return values;
		}
	}
}
