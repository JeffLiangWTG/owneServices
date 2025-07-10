using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Testing
{
	[TestedType(typeof(GLAccount))]
	class GLAccountTest : DataObjectTestCase<GLAccount>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues()
		{
			return new Dictionary<string, int>()
			{
				{ nameof(GLAccount.AccountCode), AccGLHeaderSchema.AG_AccountNum.MaxLength },
				{ nameof(GLAccount.Description), AccGLHeaderSchema.AG_Description.MaxLength },
				{ nameof(GLAccount.LocalComplianceAccountCode), AccGLAccountDescriptorSchema.AJ_LocalAccountNumber.MaxLength },
				{ nameof(GLAccount.LocalComplianceAccountDescription), AccGLAccountDescriptorSchema.AJ_AccountDescription.MaxLength },
				{ nameof(GLAccount.AccountType), AccGLHeaderSchema.AG_AccountType.MaxLength },
				{ nameof(GLAccount.ConsolidationAccountCode), AccGLHeaderSchema.AG_AccountNum.MaxLength },
				{ nameof(GLAccount.StatisticalUnit), AccGLHeaderSchema.AG_StatisticalUnits.MaxLength },
			};
		}
	}
}

