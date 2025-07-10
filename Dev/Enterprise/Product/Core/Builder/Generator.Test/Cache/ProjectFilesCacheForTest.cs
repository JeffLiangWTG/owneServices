using System.Xml;

namespace Enterprise.Builder.Generator.Cache.Testing
{
	sealed class ProjectFilesCacheForTest : ProjectFilesCache
	{
		const string projFile = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Project DefaultTargets=""Build"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"" ToolsVersion=""4.0"">
  <ItemGroup>
    <Compile Include=""Cache\GenericCache.cs"" />
    <Content Include=""Cache\Content.txt"" />
    <Compile Include=""Cache\PermissionValidityCache.cs"" />
    <Compile Include=""BusinessObjectGenerator\SingleBizObjGenerator.cs"">
      <SubType>Code</SubType>
    </Compile>
  </ItemGroup>
</Project>";

		protected override XmlDocument GetXmlDocument(string projectFileName)
		{
			var doc = new XmlDocument();
			doc.LoadXml(projFile);
			return doc;
		}
	}
}
