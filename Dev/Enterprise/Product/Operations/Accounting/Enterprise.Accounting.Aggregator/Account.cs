using System;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.ReportTableProviders
{
	public class Account
	{
		public Account()
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public Account(Guid accountPK)
		{
			string sQL = "SELECT " + AccGLHeaderSchema.Constants.AG_AccountNum + ", " +
				AccGLHeaderSchema.Constants.AG_Description + ", " +
				AccGLHeaderSchema.Constants.AG_AccountType + ", " +
				AccGLHeaderSchema.Constants.AG_DebitCredit +
				" FROM " + AccGLHeaderSchema.Constants.SqlSchemaName + "." + AccGLHeaderSchema.Constants.TableName +
				" WHERE " + AccGLHeaderSchema.Constants.PK + " = '" + accountPK + "'";

			DbCommand command = Db.Connection.Command(sQL);
			using (var reader = command.ExecuteReader())
			{
				if (reader.Read())
				{
					fPK = accountPK;
					fAccountNo = reader.GetString(0);
					fDescription = reader.GetString(1);
					fAccountType = reader.GetString(2);
					fDebitCreditType = reader.GetString(3);
				}
			}
		}

		#region Properties

		public Guid PK
		{
			get { return fPK; }
			set
			{
				if (fPK == Guid.Empty)
				{
					fPK = value;
				}
			}
		}

		public string AccountNo
		{
			get { return fAccountNo; }
			set
			{
				if (string.IsNullOrEmpty(fAccountNo))
				{
					fAccountNo = value;
				}
			}
		}

		public string Description
		{
			get { return fDescription; }
			set
			{
				if (string.IsNullOrEmpty(fDescription))
				{
					fDescription = value;
				}
			}
		}

		public double Amount
		{
			get { return fAmount; }
			set
			{
				fAmount = value;
			}
		}

		public string AccountType
		{
			get { return fAccountType; }
			set
			{
				if (string.IsNullOrEmpty(fAccountType))
				{
					fAccountType = value;
				}
			}
		}

		public string DebitCreditType
		{
			get { return fDebitCreditType; }
			set
			{
				if (string.IsNullOrEmpty(fDebitCreditType))
				{
					fDebitCreditType = value;
				}
			}
		}

		public int PostPeriod
		{
			get { return fPostPeriod; }
			set
			{
				if (fPostPeriod == 0)
				{
					fPostPeriod = value;
				}
			}
		}

		protected Guid fPK;
		protected string fAccountNo = "";
		protected double fAmount;
		protected string fDescription = "";
		protected string fAccountType = "";
		protected string fDebitCreditType = "";
		protected int fPostPeriod;

		#endregion
	}

	public class LocalAccount : Account
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public LocalAccount(Guid localAccountPK)
		{
			string sQL = @"SELECT 
							AJ_LocalAccountNumber,
							AJ_ReportCategory,
							AJ_AccountDescription,
							AJ_DebitCredit,
							AJ_PK,
							AJ_Language
							FROM " + AccGLAccountDescriptorSchema.Constants.SqlSchemaName + "." + AccGLAccountDescriptorSchema.Constants.TableName +
				@" WHERE AJ_PK = '" + localAccountPK + "'";

			DbCommand command = Db.Connection.Command(sQL);
			using (var reader = command.ExecuteReader())
			{
				if (reader.Read())
				{
					fAccountNo = reader.GetString(0);
					fDescription = reader.GetString(1);
					fAccountType = reader.GetString(2);
					fDebitCreditType = reader.GetString(3);
					fPK = reader.GetGuid(4);
					fLanguageCode = reader.GetString(5);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public LocalAccount(Guid gLHeaderPK, string language)
		{
			string sQL = @"SELECT 
							AJ_LocalAccountNumber,
							AJ_ReportCategory,
							AJ_AccountDescription,
							AJ_DebitCredit,
							AJ_PK
							FROM " + AccGLAccountDescriptorSchema.Constants.SqlSchemaName + "." + AccGLAccountDescriptorSchema.Constants.TableName +
						@" INNER JOIN " + AccGLDescriptorPivotSchema.Constants.SqlSchemaName + "." + AccGLDescriptorPivotSchema.Constants.TableName + @" ON YJ_AJ = AJ_PK And AJ_ReportType = 'COA' WHERE YJ_AG = '" + gLHeaderPK + "' AND AJ_Language = '" + language + "'";

			DbCommand command = Db.Connection.Command(sQL);
			using (var reader = command.ExecuteReader())
			{
				if (reader.Read())
				{
					fAccountNo = reader.GetString(0);
					fDescription = reader.GetString(1);
					fAccountType = reader.GetString(2);
					fDebitCreditType = reader.GetString(3);
					fPK = reader.GetGuid(4);
					fLanguageCode = language;
				}
			}
		}

		#region Properties

		public string LanguageCode
		{
			get { return fLanguageCode; }
			set
			{
				if (string.IsNullOrEmpty(fLanguageCode))
				{
					fLanguageCode = value;
				}
			}
		}

		protected string fLanguageCode = "";

		#endregion
	}
}
