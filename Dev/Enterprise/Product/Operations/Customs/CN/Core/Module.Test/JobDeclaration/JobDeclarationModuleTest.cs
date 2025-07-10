using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.Module.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.CN.Module.Testing
{
	[TestedType(typeof(JobDeclarationModule))]
	sealed class JobDeclarationModuleTest : JobDeclarationModuleAbstractTest
	{
		protected override string CountryCode => Constants.CountryCodes.China;

		protected override Type GetExpectedJobDeclarationType() => typeof(JobDeclaration);

		protected override Type GetExpectedInvoiceHeaderType() => typeof(JobComInvoiceHeader);

		protected override Type GetExpectedInvoiceLineType() => typeof(JobComInvoiceLine);

		protected override void SetJE_ApplicationCode(BaseJobDeclaration declaration)
		{
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		}

		protected override List<string> FetchHintIgnoreField
		{
			get
			{
				var result = base.FetchHintIgnoreField;
				result.Add("RemainingDaysForDeclaration"); //will always be zero if when entry headers is null
				return result;
			}
		}

		protected override bool HasFailedFetchHint(TableHitCount tableSelect)
		{
			if (tableSelect.TableName == entryHeaderTableName || tableSelect.TableName == entryNumTableName || tableSelect.TableName == OrgAddressAdditionalInfoSchema.Constants.TableName)
			{
				return false;
			}

			return tableSelect.Value > 5;
		}

		const string entryHeaderTableName = "CusEntryHeader";
		const string entryNumTableName = "CusEntryNum";
	}
}
