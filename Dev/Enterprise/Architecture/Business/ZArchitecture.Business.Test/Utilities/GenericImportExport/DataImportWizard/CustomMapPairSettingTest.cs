namespace Enterprise.ZArchitecture.DataMapping.Testing
{
	sealed class CustomMapPairSettingTest : XmlSerializableTestCase<CustomMapPairSetting>
	{
		public void TestWriteXml()
		{
			string result = Serialize(new CustomMapPairSetting() { Input = "AAA", Output = "BBB" });
			Assert(result.Contains("<Input>AAA</Input>"));
			Assert(result.Contains("<Output>BBB</Output>"));
		}

		public void TestReadXml()
		{
			string xml = "<CustomMapPairSetting><Input>AAA</Input><Output>BBB</Output></CustomMapPairSetting>";
			CustomMapPairSetting pairSetting = Deserialize(xml);
			AssertEquals("AAA", pairSetting.Input);
			AssertEquals("BBB", pairSetting.Output);
		}
	}
}
