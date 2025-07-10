using System;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(WebBranchCodeGuidRegistryDataType))]
	sealed class WebBranchCodeGuidRegistryDataTypeTest : RegistryDataTypeTestCase<WebBranchCodeGuidRegistryDataType>
	{
		[ExpectException(typeof(RegistryValidationException))]
		public void TestValidateyWithInvalidGuid()
		{
			var factory = new BusinessObjectFactory();
			var branch = factory.Load(ObjectFactory.GetType("IGlbBranch"), EnvProxy.GetAnyBranch(factory));
			branch[GlbBranchSchema.GB_WebAddress] = string.Empty;
			factory.Save();
			WebBranchDataType.Validate(NotificationGroup, branch.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty);
		}

		[ExpectException(typeof(RegistryValidationException))]
		public void TestValidateWithInactive()
		{
			var factory = new BusinessObjectFactory();
			var branch = factory.Load(ObjectFactory.GetType("IGlbBranch"), EnvProxy.GetAnyBranch(factory));
			branch[GlbBranchSchema.GB_WebAddress] = "http://webtracker";
			branch[GlbBranchSchema.GB_IsActive] = false;
			factory.Save();
			WebBranchDataType.Validate(NotificationGroup, branch.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty);
		}

		[ExpectException(typeof(RegistryValidationException))]
		public void TestValidateWithNoBranch()
		{
			WebBranchDataType.Validate(NotificationGroup, Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
		}

		protected override WebBranchCodeGuidRegistryDataType GetNewDataType()
		{
			return WebBranchDataType;
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var factory = new BusinessObjectFactory();
			var branch1 = factory.Load(ObjectFactory.GetType("IGlbBranch"), EnvProxy.GetAnyBranch(factory));
			branch1[GlbBranchSchema.GB_WebAddress] = "http://webtracker";
			branch1[GlbBranchSchema.GB_IsActive] = true;
			var branch2 = factory.NewWithValidTestData(ObjectFactory.GetType("IGlbBranch"));
			branch2[GlbBranchSchema.GB_WebAddress] = "http://anotherwebsite";
			branch2[GlbBranchSchema.GB_IsActive] = true;
			factory.Save();

			var key1 = branch1.PK.ToGuid();
			var key2 = branch2.PK.ToGuid();

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(key1, Encoding.Unicode.GetBytes(key1.ToString())),
				new ValidSampleAndBinaryValueInDB(key2, Encoding.Unicode.GetBytes(key2.ToString()))
			};
		}

		WebBranchCodeGuidRegistryDataType WebBranchDataType
		{
			get
			{
				if (webBranchDataType == null)
				{
					webBranchDataType = new WebBranchCodeGuidRegistryDataType();
				}
				return webBranchDataType;
			}
		}
		WebBranchCodeGuidRegistryDataType webBranchDataType;

		GuidRegistryItem NotificationGroup
		{
			get
			{
				if (notificationGroup == null)
				{
					notificationGroup = new GuidRegistryItem("", null, null, null, RegistryStorageFlags.All, RegistryOptions.IsValueOptional);
					notificationGroup.DataType = WebBranchDataType;
				}
				return notificationGroup;
			}
		}
		GuidRegistryItem notificationGroup;
	}
}
