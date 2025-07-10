using System.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class InterchangeNumberDetails : NonPersistentBusinessObject
	{
		public InterchangeNumberDetails(GlbCompany company) : base(company.Factory)
		{
			Company = company;
			LoadCurrentInterchangeNumber();
		}

		GlbCompany Company { get; }

		[ResourceStringData("Enterprise.Customs.AU.Declaration.Business.InterchangeNumberDetails|CompanyEDISite", Caption = "Company EDI Site")]
		public ZString CompanyEDISite => Company.GC_CustomsRegistrationNo;

		public ZPropertyInfo CompanyEDISiteInfo => GetZPropertyInfo(nameof(CompanyEDISite));

		[ResourceStringData("Enterprise.Customs.AU.Declaration.Business.InterchangeNumberDetails|CustomsEDISite", Caption = "Customs EDI Site")]
		public ZString CustomsEDISite => "AAA336C";

		public ZPropertyInfo CustomsEDISiteInfo => GetZPropertyInfo(nameof(CustomsEDISite));

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.AU.Declaration.Business.InterchangeNumberDetails|CurrentInterchangeNumber", Caption = "Current Interchange Number")]
		public ZLong CurrentInterchangeNumber
		{
			get => currentInterchangeNumber;
			set
			{
				if (currentInterchangeNumber != value)
				{
					SetNonPersistentPropertyValue(CurrentInterchangeNumberInfo, ref currentInterchangeNumber, value);
				}
			}
		}
		public ZLong currentInterchangeNumber;

		public ZPropertyInfo CurrentInterchangeNumberInfo => GetZPropertyInfo(nameof(CurrentInterchangeNumber));

		[ResourceStringData("Enterprise.Customs.AU.Declaration.Business.InterchangeNumberDetails|NewInterchangeNumber", Caption = "New Interchange Number")]
		public ZLong NewInterchangeNumber
		{
			get => newInterchangeNumber;
			set
			{
				if (newInterchangeNumber != value)
				{
					SetNonPersistentPropertyValue(NewInterchangeNumberInfo, ref newInterchangeNumber, value);
					Validation.ValidateNewInterchangeNumber();
				}
			}
		}
		ZLong newInterchangeNumber;

		public ZPropertyInfo NewInterchangeNumberInfo => GetZPropertyInfo(nameof(NewInterchangeNumber));

		public const long NewInterchangeNumberIncrement = 1_000_000L;

		public void SetNewInterchangeNumberAutomatically()
		{
			NewInterchangeNumber = CurrentInterchangeNumber + NewInterchangeNumberIncrement;
		}

		public InterchangeNumberDetailsValidation Validation => new InterchangeNumberDetailsValidation(this);

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateNewInterchangeNumber();
		}

		void AddQueryParameter(DbCommand command)
		{
			command.AddParameter("@Name", System.Data.SqlDbType.VarChar, 256, $"I{CompanyEDISite}{CustomsEDISite}");
		}

		void LoadCurrentInterchangeNumber()
		{
			try
			{
				CurrentInterchangeNumber = Db.Connection.ExecuteScalar<long>("select top 1 [SN_Value] from dbo.StmNums where [SN_Name] = @Name", cmd => AddQueryParameter(cmd));
			}
			catch (ExecuteScalarReturnedNullException) { }
		}

		public bool SaveNewInterchangeNumber()
		{
			var saved = false;
			RunPreSaveValidation();
			if (!HasErrors)
			{
				var updateSQLScript = @"DECLARE
@Owner           uniqueidentifier = '00000000-0000-0000-0000-000000000000',
@InitMinValue    bigint,
@InitNextValue   bigint,
@InitMaxValue    bigint,
@InitCanRollover bit
 
SELECT
@InitMinValue    = SN_MinimumValue,
@InitNextValue   = SN_Value,
@InitMaxValue    = SN_MaximumValue,
@InitCanRollover = SN_CanRollover
FROM
dbo.StmNums
WHERE 1=1
AND SN_Name = @Name
AND SN_Owner = @Owner
AND SN_Sequence = 0
 
BEGIN TRAN
 
BEGIN TRY
EXEC dbo.FountainSetValuesStrategy
@Name                  = @Name,
@Owner                 = @Owner,
@MinValue              = @InitMinValue,
@NextValue             = @NewNextValue,
@MaxValue              = @InitMaxValue,
@InitMinValue          = @InitMinValue,
@InitNextValue         = @InitNextValue,
@InitMaxValue          = @InitMaxValue,
@InitCanRollover       = @InitCanRollover,
@CallInSameTransaction = 1;
 
COMMIT;
END TRY BEGIN CATCH
if (@@TRANCOUNT > 0) ROLLBACK;
THROW;
END CATCH
";
				Db.Connection.ExecuteNonQuery(updateSQLScript, cmd =>
				{
					AddQueryParameter(cmd);
					cmd.AddParameter("@NewNextValue", System.Data.SqlDbType.BigInt, (long)NewInterchangeNumber);
				});
				saved = true;
				Company.Logs.AddNew(Events.ResetEntryMessageItemFunction, "Reset Interchange Block");
				Company.Factory.Save();
			}
			return saved;
		}
	}
}
