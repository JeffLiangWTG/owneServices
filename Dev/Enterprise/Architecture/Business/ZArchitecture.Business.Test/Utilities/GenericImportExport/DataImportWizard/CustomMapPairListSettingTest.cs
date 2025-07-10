namespace Enterprise.ZArchitecture.DataMapping.Testing
{
	sealed class CustomMapPairListSettingTest : XmlSerializableTestCase<CustomMapPairListSetting>
	{
		public void TestWriteXml()
		{
			var pairListSetting = new CustomMapPairListSetting { Name = "TEST", Pairs =
				[
					new() { Input = "AAA", Output = "BBB" },
					new() { Input = "CCC", Output = "DDD" }
				]
			};
			var result = Serialize(pairListSetting);
			Assert(result.Contains("<Name>TEST</Name>"));
			Assert(result.Contains("<Pair><Input>AAA</Input><Output>BBB</Output></Pair>"));
			Assert(result.Contains("<Pair><Input>CCC</Input><Output>DDD</Output></Pair>"));
		}

		public void TestReadXml()
		{
			string xml = "<CustomMapPairListSetting><Name>TEST</Name><Pair><Input>AAA</Input><Output>BBB</Output></Pair><Pair><Input>CCC</Input><Output>DDD</Output></Pair></CustomMapPairListSetting>";
			var pairListSetting = Deserialize(xml);
			AssertEquals("TEST", pairListSetting.Name);
			AssertEquals(2, pairListSetting.Pairs.Count);
			AssertEquals("AAA", pairListSetting.Pairs[0].Input);
			AssertEquals("BBB", pairListSetting.Pairs[0].Output);
			AssertEquals("CCC", pairListSetting.Pairs[1].Input);
			AssertEquals("DDD", pairListSetting.Pairs[1].Output);
		}
	}
}
