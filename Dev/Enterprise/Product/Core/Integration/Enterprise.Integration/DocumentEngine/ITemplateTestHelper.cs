namespace Enterprise.Integration.DocumentEngine
{
	public interface ITemplateTestHelper
	{
		void AddWorkSheet(string name, string contents);
		byte[] CreateTemplateBlob();
	}
}
