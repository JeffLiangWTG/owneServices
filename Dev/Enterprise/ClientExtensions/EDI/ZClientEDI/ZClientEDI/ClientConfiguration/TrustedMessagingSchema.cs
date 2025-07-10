using CargoWise.Database.Abstractions.Extensions;
using CargoWise.Database.Shared;

namespace Enterprise.Client.EDI
{
	public static class TrustedMessagingSchema
	{
		#region EdiAccessToken 

		public static DatabaseObjectCreateScript EdiAccessToken
		{
			get
			{
				return new DatabaseObjectCreateScript("EdiAccessToken", @"
CREATE TABLE dbo.EdiAccessToken(
	EAT_PK [uniqueidentifier] NOT NULL,
	EAT_Token [varchar](128) NOT NULL,
	EAT_ExpiresAtUtc [smalldatetime] NOT NULL,
	EAT_OwnerId [uniqueidentifier] NOT NULL,
	EAT_OwnerTableCode [varchar](3) NOT NULL DEFAULT (''),
	EAT_ResourceProduct [varchar](3) NOT NULL DEFAULT (''),
	EAT_ResourceSystemId [varchar](128) NOT NULL DEFAULT (''),
	EAT_Scope [varchar](max) NOT NULL DEFAULT ('')
CONSTRAINT [PK_UX__EAT_PK] PRIMARY KEY NONCLUSTERED ([EAT_PK] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)
);

CREATE UNIQUE CLUSTERED INDEX [NR_UC__EAT_Token] ON [dbo].[EdiAccessToken]
(
	[EAT_Token] ASC
)
WITH (ALLOW_PAGE_LOCKS = OFF)
;

", "DROP TABLE EdiAccessToken");
			}
		}

		#endregion

		#region Create Token

		public static DatabaseViewAndRoutineCreateScript CreateTokenScript()
		{
			return new DatabaseViewAndRoutineCreateScript("EdiCreateToken", @"
CREATE PROCEDURE EdiCreateToken
@Token				VARCHAR(128),
@ExpiresAtUtc		DATETIME = NULL,
@OwnerId			UNIQUEIDENTIFIER,
@OwnerTableCode		VARCHAR(3),
@ResourceProduct	VARCHAR(3),
@ResourceSystemId	VARCHAR(128),
@Scope				VARCHAR(MAX),
@Result				BIT OUTPUT
AS
BEGIN
	BEGIN TRY
		INSERT INTO dbo.EdiAccessToken
			(EAT_PK, EAT_Token, EAT_ExpiresAtUtc, EAT_OwnerId, EAT_OwnerTableCode, EAT_ResourceProduct, EAT_ResourceSystemId, EAT_Scope)
		VALUES
			(NEWID(), @Token, @ExpiresAtUtc, @OwnerId, @OwnerTableCode, @ResourceProduct, @ResourceSystemId, @Scope);

		SELECT @Result = 1;
	END TRY
	BEGIN CATCH
		IF (ERROR_NUMBER() = 2601 AND ERROR_MESSAGE() LIKE '%NR_UC__EAT_Token%')
			SELECT @Result = 0;
		ELSE
			THROW
	END CATCH
END
",
				"DROP PROCEDURE EdiCreateToken",
				DbRoutineType.SqlProcedureTypeDesc);
		}

		#endregion

		#region Consume Token

		public static DatabaseViewAndRoutineCreateScript ConsumeTokenScript()
		{
			return new DatabaseViewAndRoutineCreateScript("EdiConsumeToken", @"
CREATE PROCEDURE EdiConsumeToken
@Token				VARCHAR(128),
@OwnerId			UNIQUEIDENTIFIER	OUTPUT,
@OwnerTableCode		VARCHAR(3)			OUTPUT,
@ResourceProduct	VARCHAR(3)			OUTPUT,
@ResourceSystemId	VARCHAR(128)		OUTPUT,
@Scope				VARCHAR(MAX)		OUTPUT,
@Result				BIT					OUTPUT
AS
BEGIN

	SET NOCOUNT ON;

	SELECT
	TOP 1
		@OwnerId = EAT_OwnerId,
		@OwnerTableCode = EAT_OwnerTableCode,
		@ResourceProduct = EAT_ResourceProduct,
		@ResourceSystemId = EAT_ResourceSystemId,
		@Scope = EAT_Scope
	FROM
		dbo.EdiAccessToken
	WHERE
		EAT_Token = @Token
		AND EAT_ExpiresAtUtc > SYSUTCDATETIME()

	IF (@@ROWCOUNT = 0)
	BEGIN
		SET @Result = 0;
		RETURN;
	END

	SET @Result = 1;
	BEGIN
		DELETE dbo.EdiAccessToken
		WHERE EAT_Token = @Token
	END
END
",
				"DROP PROCEDURE EdiConsumeToken",
				DbRoutineType.SqlProcedureTypeDesc);
		}

		#endregion

		#region Update Secret Key

		public static DatabaseViewAndRoutineCreateScript UpdateSecretKeyScript()
		{
			return new DatabaseViewAndRoutineCreateScript("EdiUpdateSecretKey", @"
CREATE PROCEDURE EdiUpdateSecretKey
@SystemPk			UNIQUEIDENTIFIER,
@SecretKey			VARBINARY(MAX),
@SecretKeyExpiry	SMALLDATETIME
AS
BEGIN
	UPDATE
		dbo.EdiTrustedSystem
	SET
		ETS_SecretKey_COMPRESSED = @SecretKey,
		ETS_SecretKeyExpiryUtc = @SecretKeyExpiry,
		ETS_SystemLastEditTimeUtc = GETUTCDATE(),
		ETS_SystemLastEditUser = 'E'
	WHERE
		ETS_PK = @SystemPk
END
",
				"DROP PROCEDURE EdiUpdateSecretKey",
				DbRoutineType.SqlProcedureTypeDesc);
		}

		#endregion
	}
}
