CREATE PROCEDURE CreateLargeTransactionForTesting
	@NumberOfLinesToCreate int,
	@CompanyCode varchar(3),
	@BranchCode varchar(3),
	@DepartmentCode varchar(3),
	@TransactionNum varchar(38),
	@PostDate datetime,
	@OrganisationCode varchar(12) = null, 
	@AccountNumber varchar(10) = null,
	@ChargeCode varchar(10) = null,
	@CurrencyCode varchar(3) = 'EUR'
AS
BEGIN
	DECLARE @CompanyPK uniqueidentifier
	DECLARE @BranchPK uniqueidentifier
	DECLARE @DepartmentPK uniqueidentifier
	DECLARE @OrganisationPK uniqueidentifier
	DECLARE @AccountPK uniqueidentifier
	DECLARE @ChargePK uniqueidentifier
	DECLARE @CreateUser varchar(3) = 'E'
	DECLARE @TransactionHeaderPK uniqueidentifier = NEWID()
	DECLARE @ErrorCode int

	SET NOCOUNT ON

	ALTER TABLE AccTransactionLines DISABLE TRIGGER ALL

	-- Set default values for not specified arguments (use first valid code)
	SELECT @CompanyPK = GC_PK FROM dbo.GlbCompany WHERE GC_Code = @CompanyCode
	IF @OrganisationCode IS null
	BEGIN
		SELECT TOP 1 @OrganisationCode = OH_Code
			FROM dbo.OrgHeader
			JOIN dbo.OrgCompanyData ON OB_OH = OH_PK
			WHERE OH_IsActive = 1
			AND OB_IsValid = 1
			AND OB_GC = @CompanyPK
	END
	IF @AccountNumber IS null
	BEGIN
		SELECT TOP 1 @AccountNumber = AG_AccountNum
			FROM dbo.AccGLHeader
			WHERE AG_IsActive = 1
			AND AG_AccountType = 'P&L'
	END
	IF @ChargeCode IS null
	BEGIN
		SELECT TOP 1 @ChargeCode = AC_Code
			FROM dbo.AccChargeCode
			WHERE AC_GC = @CompanyPK
			AND AC_IsActive = 1
	END

	-- Get primary keys
	SELECT @BranchPK = GB_PK FROM dbo.GlbBranch WHERE GB_Code = @BranchCode
	SELECT @DepartmentPK = GE_PK FROM dbo.GlbDepartment WHERE GE_Code = @DepartmentCode
	SELECT @OrganisationPK = OH_PK FROM dbo.OrgHeader WHERE OH_Code = @OrganisationCode
	SELECT @AccountPK = AG_PK FROM dbo.AccGLHeader WHERE AG_AccountNum = @AccountNumber
	SELECT @ChargePK = AC_PK FROM dbo.AccChargeCode WHERE AC_Code = @ChargeCode

	-- Create transaction header
	INSERT INTO dbo.AccTransactionHeader (
			[AH_PK], [AH_Ledger], [AH_TransactionType], [AH_TransactionNum], [AH_TransactionCount],
			[AH_TransactionReference], [AH_Desc], [AH_InvoiceDate], [AH_TransactionCategory], [AH_DueDate],
			[AH_InvoiceAmount], [AH_GSTAmount], [AH_WithholdingTax], [AH_OSTotal], [AH_ExchangeRate],
			[AH_AgePeriod], [AH_PostPeriod], [AH_PostDate], [AH_ReceiptType], [AH_ChequeDrawer],
			[AH_DrawerBank], [AH_DrawerBranch], [AH_ConsolidatedInvoiceRef], [AH_FullyPaidDate], [AH_OutstandingAmount],
			[AH_PostToGL], [AH_InvoiceTerm], [AH_InvoiceTermDays], [AH_ExportBatchNumber], [AH_OH],
			[AH_GB], [AH_GE], [AH_AG], [AH_GC], [AH_RX_NKTransactionCurrency],
			[AH_SystemCreateTimeUtc], [AH_SystemCreateUser], [AH_RequisitionStatus], [AH_NumberOfSupportingDocuments], [AH_ComplianceSubType],
			[AH_SystemLastEditTimeUtc], [AH_SystemLastEditUser], [AH_CashBasisGSTIndicator], [AH_CashBasisGSTRealisedToGL], [AH_InvoiceApproved],
			[AH_InvoicePrinted], [AH_IsCancelled], [AH_NotAllocated], [AH_PostedToEFT], [AH_TransactionCreatedByMatching],
			[AH_POST1], [AH_POST2], [AH_POST3], [AH_POST4], [AH_PostedInternal],
			[AH_ReceiptBatchNo], [AH_ChequeOrReference], [AH_AgreedPaymentMethodOverride], [AH_GS_NKAuditedBy], [AH_GS_NKCashier],
			[AH_InvoicePaymentReferenceCode], [AH_LocalTaxAmountOtherTaxes], [AH_OSTaxAmountOtherTaxes], [AH_PlaceOfSupply], [AH_PlaceOfSupplyType],
			[AH_MatchStatus], [AH_MatchStatusReasonCode], [AH_OriginalTransactionNum], [AH_AutoVersion], [AH_JobNumber]
		) VALUES (
			@TransactionHeaderPK, 'AP', 'INV', @TransactionNum, 1,
			'', 'Test Transaction', @PostDate, 'STD', @PostDate,
			@NumberOfLinesToCreate, 0, 0, @NumberOfLinesToCreate, 1,
			0, 0, @PostDate, '', '',
			'', '', '', @PostDate, 0,
			'N', '', 0, 0, @OrganisationPK,
			@BranchPK, @DepartmentPK, @AccountPK, @CompanyPK, @CurrencyCode,
			GETUTCDATE(), @CreateUser, '', 1, '',
			GETUTCDATE(), @CreateUser, 0, 0, 0,
			0, 0, 0, 0, 0,
			0, 0, 0, 0, 0,
			'', '', '','', '',
			'', 0, 0, '', '',
			'', '', '', 0, ''
		)
	SET @ErrorCode = @@ERROR

	-- Create transaction lines
	IF @ErrorCode = 0
	BEGIN
		INSERT INTO dbo.AccTransactionLines (
				[AL_PK], [AL_LineType], [AL_Sequence], [AL_Desc], [AL_LineAmount],
				[AL_GSTVAT], [AL_WithholdingTax], [AL_UnitQty], [AL_UnitPrice], [AL_OSUnitPrice],
				[AL_OSAmount], [AL_ExchangeRate], [AL_PostPeriod], [AL_PostDate], [AL_PostToGL],
				[AL_ReversePeriod], [AL_ReverseDate], [AL_ReverseToGL], [AL_ExportBatchNumber], [AL_ExportReverseBatchNumber],
				[AL_AH], [AL_AC], [AL_GE], [AL_GB], [AL_AG],
				[AL_PercentageOfPeriod], [AL_RX_NKTransactionCurrency], [AL_RevRecognitionType], [AL_SystemCreateTimeUtc], [AL_SystemCreateUser],
				[AL_SystemLastEditTimeUtc], [AL_SystemLastEditUser], [AL_GSTVATBasis], [AL_PreventInvoicePrintGrouping], [AL_IsFinalCharge],
				[AL_InputGSTVATRecoverable], [AL_GC], [AL_GovtChargeCode], [AL_GSTVATExtra], [AL_TaxDate],
				[AL_TaxExtraRateDenominator], [AL_TaxExtraRateNumerator], [AL_TaxRateDenominator], [AL_TaxRateNumerator], [AL_PlaceOfSupply],
				[AL_PlaceOfSupplyType], [AL_AutoVersion], [AL_SupplyType]
			) SELECT TOP (@NumberOfLinesToCreate)
				NEWID(), 'CST', 1, 'Position ' + CAST(CAST(ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS INT) as varchar), 1,
				0, 0, 0, 0, 0,
				1, 1, 0, @PostDate, 'N',
				0, @PostDate, 'N', 0, 0,
				@TransactionHeaderPK, @ChargePK, @DepartmentPK, @BranchPK, @AccountPK,
				0, @CurrencyCode, 'IMM', GETUTCDATE(), @CreateUser,
				GETUTCDATE(), @CreateUser, 'A', 0, 0,
				1, @CompanyPK, '', 0, @PostDate,
				1, 0, 1, 0, '',
				'', 0, ''
			FROM dbo.StmNumberSequence a
			CROSS JOIN dbo.StmNumberSequence b
		SET @ErrorCode = @@ERROR
	END

	ALTER TABLE AccTransactionLines ENABLE TRIGGER ALL

	SET NOCOUNT OFF

	IF @ErrorCode = 0
	BEGIN
		PRINT 'Successfully created transaction number ' + @TransactionNum + ' with ' + CAST(@NumberOfLinesToCreate as varchar) + ' lines.'
		SELECT @TransactionHeaderPK AS 'AH_PK'
	END
END
