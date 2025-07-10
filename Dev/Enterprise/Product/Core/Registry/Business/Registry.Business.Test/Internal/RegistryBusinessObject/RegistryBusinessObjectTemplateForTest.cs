using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Testing
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.Test.XmlSerializers")]
	sealed class RegistryBusinessObjectTemplateForTest : RegistryBusinessObjectTemplate
	{
		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			RegistryBusinessObjectTemplate result = new RegistryBusinessObjectTemplateForTest();
			if (SuspendValidationOnNewObjectForCloning)
			{
				result.SuspendValidation();
			}
			return result;
		}

		[CargoWise.ComponentModel.MaxLength(10)]
		public ZString TestString
		{
			get { return fTestString; }
			set
			{
				CheckMaximumLength(TestStringInfo, value);
				fTestString = value;

				if (!IsValidationSuspended)
				{
					ValidateTestString();
				}
			}
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
		}

		public ZPropertyInfo TestStringInfo
		{
			get { return GetZPropertyInfo(nameof(TestString)); }
		}

		void ValidateTestString()
		{
			ValidateTestStringCalled = true;
		}

		public bool ValidateTestStringCalled;
		public bool SuspendValidationOnNewObjectForCloning;
		ZString fTestString;
	}
}
