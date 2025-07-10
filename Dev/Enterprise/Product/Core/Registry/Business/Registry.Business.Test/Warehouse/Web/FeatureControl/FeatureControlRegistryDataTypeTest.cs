using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(FeatureControlRegistryDataType))]
	sealed class FeatureControlRegistryDataTypeTest : RegistryDataTypeTestCase<FeatureControlRegistryDataType>
	{
		protected override FeatureControlRegistryDataType GetNewDataType()
		{
			return new FeatureControlRegistryDataType(isEncrypted: true);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var data1 = @"H4sIAAAAAAAEAIWQUWvCMBSF3wf7DyXPs0mropQa2TodAx2j7XA4hoR60UDblCS17t8v1uIMbAxCuDf3O4d7Ek6PRe4cQCouygnyXIKm9PYmnAPTtYRIlFqK3DFQqYKj2k7QXusqwLhpGrfpu0LusE+Ih9+XiyTbQ8HQBeb/wz1eKs3KDDqVpYAtdzNRuKzGs1KDrCRXkIA88AwwMls6TpjyAoxDUb3pjPrEH/bIoOcNUjIO/FHQJ+64P1qH2MJa4X2ei8ZcXdBEsx0oqmUNIf591sriOu/Krulq082j5cb+tkhsgUbxMJ4lq2ezxR/ElUO8OXmmXxXQp8XDSXH1YnFmKakfmYaf4F4bnATtWZ/FFmYZvDLJCjDfqugHilYeunPQ7CVFn2fh1bhLiy9xz+VpYFgrD/0GnYLKZVACAAA=";
			var data2 = @"H4sIAAAAAAAEAIWQYUvDMBCGvwv+h5LPrknnxrR0GbNuIjiVtjKZyAjdsQXapiTpOv+9aVfmAooQkrvc8x73XjA55JmzB6m4KMbIcwma0MuLYA5MVxJCUWgpMsdAhfIPajNGO61LH+O6rt362hVyi/uEePh98RSnO8gZOsH8f7jHC6VZkUKnshSw4W4qcpdVeFZokKXkCmKQe54CRmZKxwkSnoPpkJdvOqV90h/2yKDnDRJy65OR7xH3ZjBaBdjCWuE0y0Rtrs5orNkWFNWyggD/XmtlUZV1YZd0scnm4WJtry0UG6BhNIxm8fLRTPEHcdYhWjc9k68S6MPTXaM4+7E4M5TU90zDj3GvNU789qyOYguzGrwyyXIwa1X0A4VLD105aPacNM80fEGfR/0Z1ZnGJ9fHsCkY1rJFvwE+4+bQVwIAAA==";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(data1, GetNewDataType().Serialise(data1)),
				new ValidSampleAndBinaryValueInDB(data2, GetNewDataType().Serialise(data2)),
			};
		}

		protected override object GetNullRepresentation()
		{
			return GetNewDataType().Serialise("*** NULL ***");
		}

		public void TestValidation()
		{
			var dataType = new FeatureControlRegistryDataType();
			var stringRegistryItem = new StringRegistryItem("FeatureControlRuleContent", null, null, null, RegistryStorageFlags.System);
			var invalidContent = "Invalid Content";
			AssertExceptionThrown(typeof(RegistryValidationException), "Invalid Feature Control String Setting", () => dataType.Validate(stringRegistryItem, invalidContent, Guid.Empty, Guid.Empty, Guid.Empty));

			var validContent = @"H4sIAAAAAAAEAIWQb2vCMBDG3w/2HUpezyb1L5QacZ2OgW6j7XA4hoR6aKBtSpJa/fZLtYiBjUFI7nK/57jngskxz5wDSMVFMUaeS9CE3t8Fc2C6khCKQkuROQYqlH9U2zHaa136GNd17dY9V8gd7hLi4c/lIk73kDN0hfn/cIcXSrMihVZlKWDL3VTkLqvwrNAgS8kVxCAPPAWMzJSOEyQ8B9MhLz90SrukO+gQr+MNEzL0Sc8fELc/Gq0DbGFn4TTLRG2u1mis2Q4U1bKCAP9eO8uiKmvDNmljk83D5cZeWyi2QMNoEM3i1YuZ4g/ipkO0aXompxLo8+KxUdz8WJwZSuonpsEy3k8I8c9nfRFbmNXgnUmWg1mrol8oXHnowUGz16R5puEb+r7ob6jWNL66voRNwbCWLfoDUAFdz1cCAAA=";
			AssertNoExceptionThrown(() => dataType.Validate(stringRegistryItem, validContent, Guid.Empty, Guid.Empty, Guid.Empty));
		}
	}
}
