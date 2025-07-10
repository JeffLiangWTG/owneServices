using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;
using ZClientEDI.Business.Registry;

namespace ZClientEDI.Business.Test.Registry.Telematics
{
	[TestedType(typeof(TcaRimEnrollmentSchemeRegistryItem))]
	public class TcaRimEnrollmentSchemeRegistryItemTest : StronglyTypedRegistryItemTestCase<CodeDescriptionBoolCollection, TcaRimEnrollmentSchemeCollection>
	{
		protected override StronglyTypedRegistryItem<CodeDescriptionBoolCollection, TcaRimEnrollmentSchemeCollection> GetNewRegistryItem()
		{
			CodeDescriptionBoolRegistryEditorInfo editorInfo = new CodeDescriptionBoolRegistryEditorInfo((NoResString)"Active");
			return new TcaRimEnrollmentSchemeRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, 16, editorInfo, new TcaRimEnrollmentSchemeCollection());
		}
	}
}
