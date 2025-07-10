using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.GLAccountFormat
{
	public class GLAccountFormatter : NonPersistentBusinessObject, IObsoleteValidation
	{
		public GLAccountFormatter()
			: base(new BusinessObjectFactory())
		{
		}

		public void Update(string updatedMask)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			AccGLHeader[] headers = factory.Load<AccGLHeader>(new ZQuery());
			foreach (AccGLHeader header in headers)
			{
				header.AG_AccountNum = ConvertNumber(header.AG_AccountNum, updatedMask);
			}

			using (var manager = ((IDbConnected)factory).Connection.BeginTransactionWithManager())
			{
				factory.Save();
				AccountingConfigurationRegistry.Instance.GLAccountFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, updatedMask);
				manager.CommitTransaction();
			}
		}

		#region Current Format

		[MaxLength(10)]
		public ZString CurrentFormat
		{
			get { return CurrentFormatCore; }
		}

		public ZPropertyInfo CurrentFormatInfo
		{
			get { return GetZPropertyInfo(nameof(CurrentFormat)); }
		}

		protected virtual ZString CurrentFormatCore
		{
			get
			{
				if (currentFormat == null)
				{
					currentFormat = AccountingConfigurationRegistry.Instance.GLAccountFormat.Value;
				}
				return currentFormat;
			}
		}

		string currentFormat;

#if DEBUG
		public IEnumerable<ZString> NotMatchNumberList_ForTestOnly;
#endif

		public void ValidateGLAccountNumLength()
		{
			CurrentFormatInfo.ClearAllNotifications();

			if (this.HasErrors())
			{
				return;
			}

			var maskLength = NewFormat.ToString().Count(c => c == 'X');
			var query = new ZDBOnlyQuery(typeof(AccGLHeader));
			query.MaximumRows = 3;
			query.AddFilterAndZSQLParameterCollection(FormattableString.Invariant($"LEN(REPLACE({AccGLHeader.Schema.AG_AccountNum}, '.', '')) < {maskLength}"), null);
			var notMatchNumberList = new BusinessObjectFactory().Load<AccGLHeader>(query).Select(x => x.AG_AccountNum);

#if DEBUG
			NotMatchNumberList_ForTestOnly = notMatchNumberList;
#endif

			if (notMatchNumberList.Any())
			{
				var message = new ZStringBuilder(Res.GetString("5E443C1E-181F-49BE-95E6-2F88DA901E38", @"The current GL Account format is {0}.
The following GL accounts do not comply with this format:", CurrentFormat));
				foreach (var notMatchNumber in notMatchNumberList)
				{
					message.Append(notMatchNumber);
				}
				message.Append(Res.GetString("B6D8DB62-92D0-467B-83C1-725F28A99988", @"… …
Please review and ensure that all GL Accounts under Maintain > Account > GL Accounts comply to the current GL Account Format before changing to the new GL Account Format."));
				CurrentFormatInfo.AddError(message.ToStringWithNewLineBetweenAppends());
			}
		}

		#endregion

		#region New Format

		[MaxLength(10)]
		public ZString NewFormat
		{
			get { return newFormat; }
			set
			{
				if (newFormat != value)
				{
					CheckMaximumLength(NewFormatInfo, value);
					SetNonPersistentPropertyValue(NewFormatInfo, ref newFormat, value);
					if (!IsValidationSuspended)
					{
						ValidateNewFormat();
					}
				}
			}
		}

		public ZPropertyInfo NewFormatInfo
		{
			get { return GetZPropertyInfo(nameof(NewFormat)); }
		}

		public void ValidateNewFormat()
		{
			NewFormatInfo.ClearAllNotifications();

			if (NewFormat.IndexOf("..") != -1)
			{
				NewFormatInfo.AddError(Res.GetString("48faafef-0fd6-403b-918d-6bb36a118992", "Two dots next to each other are not allowed."));
			}
			if (NewFormat.Replace("X", "").Replace(".", "").Length > 0)
			{
				NewFormatInfo.AddError(Res.GetString("628b0468-0d88-4733-a7a3-f213bc21064e", "Only X and . are valid symbols."));
			}
			if (!NewFormat.StartsWith("X") || !NewFormat.EndsWith("X"))
			{
				NewFormatInfo.AddError(Res.GetString("1a4fc944-2e18-438b-a9fb-0cb3ff4bff5d", "The GL format string should start and end with X."));
			}
			if (NewFormat.Replace(".", "").Length < CurrentFormat.Replace(".", "").Length)
			{
				NewFormatInfo.AddError(Res.GetString("b6e03dae-6dc3-4fd4-b0e5-d98615739d4d", "Cannot decrease the amount of X in the string."));
			}

			int currentDots = CurrentFormat.KeepChars(".", "").Length;
			int newDots = NewFormat.KeepChars(".", "").Length;
			if (newDots == 0 || newDots < currentDots || newDots > 2)
			{
				string message = (currentDots == 2) ?
					Res.GetString("9aa8c10a-66e1-4da5-97f7-b201a33810ee", "The GL format string must have 2 dots in the string.") : Res.GetString("9c2b433a-da80-4087-86e3-47ddabb0a878", "The GL format string must have 1 or 2 dots in the string.");
				NewFormatInfo.AddError(message);
			}
		}

		ZString newFormat;

		#endregion

		#region Implementation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateAll();
		}

		public void ValidateAll()
		{
			ValidateNewFormat();
			//For performance reason, make sure ValidateGLAccountNumLength the last one to validate. It will read database.
			ValidateGLAccountNumLength();
		}

		string ConvertNumber(string input, string updatedMask)
		{
			input = input.Replace(".", "");

			string result = "";
			char[] newMask = updatedMask.ToCharArray();
			char[] currentValue = input.ToCharArray();

			int index = 0;

			for (int i = 0; i < newMask.Length; i++)
			{
				if (newMask[i] == 'X')
				{
					result += currentValue[index];
					index++;
				}
				else
				{
					result += newMask[i];
				}
			}

			return result;
		}

		#endregion
	}
}

