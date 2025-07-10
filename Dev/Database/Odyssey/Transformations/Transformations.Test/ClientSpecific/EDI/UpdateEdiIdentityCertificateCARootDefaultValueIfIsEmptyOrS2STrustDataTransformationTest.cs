using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.ClientSpecific.EDI
{
	[TestedType(typeof(UpdateEdiIdentityCertificateCARootDefaultValueIfIsEmptyOrS2STrustDataTransformation))]
	public class UpdateEdiIdentityCertificateCARootDefaultValueIfIsEmptyOrS2STrustDataTransformationTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			AssertEquals(4, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.EdiIdentityCertificate WHERE ICE_CARoot = 'S2S'"));
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new UpdateEdiIdentityCertificateCARootDefaultValueIfIsEmptyOrS2STrustDataTransformation();

		protected override void PrepareTestData()
		{
			DbObjectCreator.CreateTableIfNotExists(Db.Connection, "EdiIdentityApplication", @"
Create Table dbo.EdiIdentityApplication
(
IDA_PK uniqueidentifier NOT NULL,
IDA_LD uniqueidentifier NULL,
IDA_ClientID varchar(36) NOT NULL DEFAULT '',
IDA_IsRollback bit NOT NULL Default 0,
IDA_ApplicationName varchar(256) NOT NULL DEFAULT '',
IDA_RedirectUrlStatus varchar(3) NOT NULL DEFAULT 'NON',
IDA_RedirectUrlLastSyncTimeUtc datetime NULL,
IDA_IsActive bit NOT NULL Default 1,
IDA_SystemCreateUser varchar(3) NOT NULL DEFAULT '',
IDA_SystemCreateTimeUtc smalldatetime NULL,
IDA_SystemLastEditUser varchar(3) NOT NULL DEFAULT '',
IDA_SystemLastEditTimeUtc datetime NULL
);");

			DbObjectCreator.CreateTableIfNotExists(Db.Connection, "EdiIdentityCertificate", @"
CREATE TABLE dbo.EdiIdentityCertificate
(
ICE_PK uniqueidentifier NOT NULL,
ICE_IDA uniqueidentifier NOT NULL,
ICE_SequenceNumber bigint NOT NULL Default 0,
ICE_CARoot varchar(20) NOT NULL DEFAULT '',
ICE_CertificateData varbinary(MAX) NULL,
ICE_CertificateThumbprint varchar(50) NULL,
ICE_CertificateValidDate smalldatetime NULL,
ICE_CertificateExpiryDate smalldatetime NULL,
ICE_CertificateIssuedTo varchar(256) NOT NULL DEFAULT '',
ICE_CertificateIssuedBy varchar(256) NOT NULL DEFAULT '',
ICE_ProcessingStatus varchar(3) NOT NULL DEFAULT 'QUE',
ICE_IsActive bit NOT NULL Default 1,
ICE_IsCertificateRevoked bit NOT NULL Default 0,
ICE_CertificateSigningRequest varchar(max) NOT NULL,
ICE_CertificateSigningRequestHash AS HASHBYTES('SHA2_256', ICE_CertificateSigningRequest),
ICE_SystemCreateTimeUtc smalldatetime NULL,
ICE_SystemCreateUser varchar(3) NOT NULL DEFAULT '',
ICE_SystemLastEditTimeUtc datetime NULL,
ICE_SystemLastEditUser varchar(3) NOT NULL DEFAULT ''
);");

			var sql = $@"INSERT INTO dbo.EdiIdentityApplication(IDA_PK,IDA_ApplicationName,IDA_SystemCreateUser,IDA_SystemCreateTimeUtc,IDA_SystemLastEditUser,IDA_SystemLastEditTimeUtc) VALUES ('481E1AFE-72E5-4E8D-B500-0631C4233C72', 'Test','E',SYSDATETIME(),'E',SYSDATETIME());
INSERT INTO dbo.EdiIdentityCertificate (ICE_PK, ICE_IDA, ICE_ProcessingStatus, ICE_CertificateSigningRequest,ICE_SequenceNumber,ICE_SystemCreateUser,ICE_SystemCreateTimeUtc,ICE_SystemLastEditUser,ICE_SystemLastEditTimeUtc) VALUES ('{certificatePK1}', '481E1AFE-72E5-4E8D-B500-0631C4233C72', 'QUE', '{Csr}', 0, 'E',SYSDATETIME(),'E',SYSDATETIME());
INSERT INTO dbo.EdiIdentityCertificate (ICE_PK, ICE_IDA, ICE_ProcessingStatus, ICE_CertificateSigningRequest,ICE_SequenceNumber,ICE_SystemCreateUser,ICE_SystemCreateTimeUtc,ICE_SystemLastEditUser,ICE_SystemLastEditTimeUtc) VALUES ('{certificatePK2}', '481E1AFE-72E5-4E8D-B500-0631C4233C72', 'QUE', '{Csr2}', 1, 'E',SYSDATETIME(),'E',SYSDATETIME());
INSERT INTO dbo.EdiIdentityCertificate (ICE_PK, ICE_IDA, ICE_ProcessingStatus, ICE_CARoot, ICE_CertificateSigningRequest,ICE_SequenceNumber,ICE_SystemCreateUser,ICE_SystemCreateTimeUtc,ICE_SystemLastEditUser,ICE_SystemLastEditTimeUtc) VALUES ('{certificatePK3}', '481E1AFE-72E5-4E8D-B500-0631C4233C72', 'QUE', 'S2STrust','{Csr3}', 2, 'E',SYSDATETIME(),'E',SYSDATETIME())
INSERT INTO dbo.EdiIdentityCertificate (ICE_PK, ICE_IDA, ICE_ProcessingStatus, ICE_CARoot, ICE_CertificateSigningRequest,ICE_SequenceNumber,ICE_SystemCreateUser,ICE_SystemCreateTimeUtc,ICE_SystemLastEditUser,ICE_SystemLastEditTimeUtc) VALUES ('{certificatePK4}', '481E1AFE-72E5-4E8D-B500-0631C4233C72', 'QUE', 'S2S','{Csr4}', 3, 'E',SYSDATETIME(),'E',SYSDATETIME())";
			Db.Connection.ExecuteNonQuery(sql);
		}

		Guid certificatePK1 = Guid.NewGuid();
		Guid certificatePK2 = Guid.NewGuid();
		Guid certificatePK3 = Guid.NewGuid();
		Guid certificatePK4 = Guid.NewGuid();

		const string Csr = @"-----BEGIN CERTIFICATE REQUEST-----
MIIC0jCCAboCAQAwazELMAkGFDSLFKSDFLDSJFLKDSJFLAoMCGpheXd0Z0NBMRww
GgYDVQQLDBNJZGVudGl0eUFuZFNlY3VyaXR5MQ8wDQYDVQQIDAZTeWRuZXkxDDAK
BgNVBAMMA0NBMTEMMAoGA1UEBwwDV1RHMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8A
MIIBCgKCAQEAinpVLEBYZdwzwC7OAOy9WT7bBqlFQU0bFWONu2WGXBblXJw1j3+e
I+GerBPO6ZwbNXSo+vr0q6dDLlD5pJsP3Rld6UwB4gquJyJfAGIuA8SQcJrYzkl6
xq3f8gL3q65f8HS/UiP+exO5VocCGjQhhvjPbsSZjF4FIWsOMb9E7pvNQ+MbJ+qs
MEIK3HO2LtgwiQdNZeUxZR4TJN7Vv3iimWG3QSAl1Q7dz4iT/dD4zId4xKK+yXPk
dczgfZQN5LnQ9Nt1/Qi97Xj8Wbu5VOSpeTb8eDP1Jezhq64Xsm7W27U6SYemF2gi
Ef5HLxt4A04XFiPbgnbm7xHArm86TnQ3AQIDAQABoCIwIAYJKoZIhvcNAQkOMRMw
ETAPBgNVHRMBAf8EBTADAQH/MA0GCSqGSIb3DQEBCwUAA4IBAQAlzcyPQj5P1iRe
HkTInm8x7lKnfbsuphlyg+HD2j2AGWtKwCR532piCTA0E2zUKqh+E0GO68eH6ou2
YbIzGk/MTUVJ2xZkYUuCNhXEdChfj0xou5Z7QFo+kZ5QUd+fQ08Jp/lZGMZrFGmc
AfwU+hn6ySqY0lFf4mSVY7hUUjPBIkO6YREhvEHuy/ShIjmbjNXCntjkd5zW8gV4
GUE/1cXy10RU86fZj6arGToDL2W7bbtfo6fu+EhNi66sawn5ipIDJ1ab91zn7iOt
yQnZCmN3mvB9tS1PH8KWEYJoYHdXojaC88IuC4xdSb8X4d9vuMtwidQEz7MpL9Li
NBmVuj0B
-----END CERTIFICATE REQUEST-----";

		const string Csr2 = @"-----BEGIN CERTIFICATE REQUEST-----
MIIC0jCCAboCAQAwazELMAkGFDSLFKSDFLDSJFLKDSJFLAoMCGpheXd0Z0NBMRww
GgYDVQQLDBNJZGVudGl0eUFuZFNlY3VyaXR5MQ8wDQYDVQQIDAZTeWRuZXkxDDAK
BgNVBAMMA0NBMTEMMAoGA1UEBwwDV1RHMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8A
MIIBCgKCAQEAinpVLEBYZdwzwC7OAOy9WT7bBqlFQU0bFWONu2WGXBblXJw1j3+e
I+GerBPO6ZwbNXSo+vr0q6dDLlD5pJsP3Rld6UwB4gquJyJfAGIuA8SQcJrYzkl6
xq3f8gL3q65f8HS/UiP+exO5VocCGjQhhvjPbsSZjF4FIWsOMb9E7pvNQ+MbJ+qs
MEIK3HO2LtgwiQdNZeUxZR4TJN7Vv3iimWG3QSAl1Q7dz4iT/dD4zId4xKK+yXPk
dczgfZQN5LnQ9Nt1/Qi97Xj8Wbu5VOSpeTb8eDP1Jezhq64Xsm7W27U6SYemF2gi
Ef5HLxt4A04XFiPbgnbm7xHArm86TnQ3AQIDAQABoCIwIAYJKoZIhvcNAQkOMRMw
ETAPBgNVHRMBAf8EBTADAQH/MA0GCSqGSIb3DQEBCwUAA4IBAQAlzcyPQj5P1iRe
HkTInm8x7lKnfbsuphlyg+HD2j2AGWtKwCR532piCTA0E2zUKqh+E0GO68eH6ou2
YbIzGk/MTUVJ2xZkYUuCNhXEdChfj0xou5Z7QFo+kZ5QUd+fQ08Jp/lZGMZrFGmc
AfwU+hn6ySqY0lFf4mSVY7hUUjPBIkO6YREhvEHuy/ShIjmbjNXCntjkd5zW8gV4
GUE/1cXy10RU86fZj6arGToDL2W7bbtfo6fu+EhNi66sawn5ipIDJ1ab91zn7iOt
yQnZCmN3mvB9tS1PH8KWEYJoYHdXojaC88IuC4xdSb8X4d9vuMtwidQEz7MpL9Li
NBmVuj0C
-----END CERTIFICATE REQUEST-----";

		const string Csr3 = @"-----BEGIN CERTIFICATE REQUEST-----
MIIC0jCCAboCAQAwazELMAkGFDSLFKSDFLDSJFLKDSJFLAoMCGpheXd0Z0NBMRww
GgYDVQQLDBNJZGVudGl0eUFuZFNlY3VyaXR5MQ8wDQYDVQQIDAZTeWRuZXkxDDAK
BgNVBAMMA0NBMTEMMAoGA1UEBwwDV1RHMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8A
MIIBCgKCAQEAinpVLEBYZdwzwC7OAOy9WT7bBqlFQU0bFWONu2WGXBblXJw1j3+e
I+GerBPO6ZwbNXSo+vr0q6dDLlD5pJsP3Rld6UwB4gquJyJfAGIuA8SQcJrYzkl6
xq3f8gL3q65f8HS/UiP+exO5VocCGjQhhvjPbsSZjF4FIWsOMb9E7pvNQ+MbJ+qs
MEIK3HO2LtgwiQdNZeUxZR4TJN7Vv3iimWG3QSAl1Q7dz4iT/dD4zId4xKK+yXPk
dczgfZQN5LnQ9Nt1/Qi97Xj8Wbu5VOSpeTb8eDP1Jezhq64Xsm7W27U6SYemF2gi
Ef5HLxt4A04XFiPbgnbm7xHArm86TnQ3AQIDAQABoCIwIAYJKoZIhvcNAQkOMRMw
ETAPBgNVHRMBAf8EBTADAQH/MA0GCSqGSIb3DQEBCwUAA4IBAQAlzcyPQj5P1iRe
HkTInm8x7lKnfbsuphlyg+HD2j2AGWtKwCR532piCTA0E2zUKqh+E0GO68eH6ou2
YbIzGk/MTUVJ2xZkYUuCNhXEdChfj0xou5Z7QFo+kZ5QUd+fQ08Jp/lZGMZrFGmc
AfwU+hn6ySqY0lFf4mSVY7hUUjPBIkO6YREhvEHuy/ShIjmbjNXCntjkd5zW8gV4
GUE/1cXy10RU86fZj6arGToDL2W7bbtfo6fu+EhNi66sawn5ipIDJ1ab91zn7iOt
yQnZCmN3mvB9tS1PH8KWEYJoYHdXojaC88IuC4xdSb8X4d9vuMtwidQEz7MpL9Li
NBmVuj0D
-----END CERTIFICATE REQUEST-----";

		const string Csr4 = @"-----BEGIN CERTIFICATE REQUEST-----
MIIC0jCCAboCAQAwazELMAkGFDSLFKSDFLDSJFLKDSJFLAoMCGpheXd0Z0NBMRww
GgYDVQQLDBNJZGVudGl0eUFuZFNlY3VyaXR5MQ8wDQYDVQQIDAZTeWRuZXkxDDAK
BgNVBAMMA0NBMTEMMAoGA1UEBwwDV1RHMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8A
MIIBCgKCAQEAinpVLEBYZdwzwC7OAOy9WT7bBqlFQU0bFWONu2WGXBblXJw1j3+e
I+GerBPO6ZwbNXSo+vr0q6dDLlD5pJsP3Rld6UwB4gquJyJfAGIuA8SQcJrYzkl6
xq3f8gL3q65f8HS/UiP+exO5VocCGjQhhvjPbsSZjF4FIWsOMb9E7pvNQ+MbJ+qs
MEIK3HO2LtgwiQdNZeUxZR4TJN7Vv3iimWG3QSAl1Q7dz4iT/dD4zId4xKK+yXPk
dczgfZQN5LnQ9Nt1/Qi97Xj8Wbu5VOSpeTb8eDP1Jezhq64Xsm7W27U6SYemF2gi
Ef5HLxt4A04XFiPbgnbm7xHArm86TnQ3AQIDAQABoCIwIAYJKoZIhvcNAQkOMRMw
ETAPBgNVHRMBAf8EBTADAQH/MA0GCSqGSIb3DQEBCwUAA4IBAQAlzcyPQj5P1iRe
HkTInm8x7lKnfbsuphlyg+HD2j2AGWtKwCR532piCTA0E2zUKqh+E0GO68eH6ou2
YbIzGk/MTUVJ2xZkYUuCNhXEdChfj0xou5Z7QFo+kZ5QUd+fQ08Jp/lZGMZrFGmc
AfwU+hn6ySqY0lFf4mSVY7hUUjPBIkO6YREhvEHuy/ShIjmbjNXCntjkd5zW8gV4
GUE/1cXy10RU86fZj6arGToDL2W7bbtfo6fu+EhNi66sawn5ipIDJ1ab91zn7iOt
yQnZCmN3mvB9tS1PH8KWEYJoYHdXojaC88IuC4xdSb8X4d9vuMtwidQEz7MpL9Li
NBmVuj0D
-----END CERTIFICATE REQUEST-----";
	}
}
