using System;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

#if DEBUG

namespace Enterprise.Testing
{
	public partial class LoadPreviousDatTestForm : ZChildForm
	{
		public Guid UserTestPK
		{
			get { return userTestPK; }
		}
		Guid userTestPK;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			this.InitializeComponent();
		}

		static bool TryFindGuidInString(string str, out Guid userTestPK)
		{
			userTestPK = Guid.Empty;
			var parts = str.Split(); //splits on all whitespace!
			foreach (var part in parts)
			{
				if (part.Length >= 36)
				{
					if (Guid.TryParse(((ZString)part).Right(36), out userTestPK))
					{
						return true;
					}
				}
			}
			return false;
		}

		static void SaveGuid(Guid guid)
		{
			var sql = string.Format(CultureInfo.InvariantCulture, @"IF EXISTS
(select 1 from dbo.StmData where SD_Name = 'LoadPreviousTestFormGuid')
BEGIN
UPDATE dbo.StmData
SET SD_GuidValue = '{0}'
WHERE SD_Name = 'LoadPreviousTestFormGuid'
END
ELSE
BEGIN
INSERT into dbo.StmData (SD_PK, SD_Name, SD_GuidValue, SD_Type, SD_IsCancelled, SD_IsLogged)
VALUES (newid(), 'LoadPreviousTestFormGuid', '{0}', 'GID', 0, 0)
END", guid.ToString());
			Db.Connection.Command(sql).ExecuteNonQuery();
		}

#if !WINZOR
		static Guid? LoadGuid()
		{
			var sql = "SELECT SD_GuidValue FROM dbo.StmData WHERE SD_Name = 'LoadPreviousTestFormGuid'";
			var result = Db.Connection.Command(sql).ExecuteScalar();
			if (result is Guid guid)
			{
				return guid;
			}
			return null;
		}
#endif

		void loadTestsButton_Click(object sender, EventArgs e)
		{
			userTestPK = Guid.Empty;
			if (!string.IsNullOrEmpty(UserTestPKTextBox.Text))
			{
				var text = UserTestPKTextBox.Text;
				if (TryFindGuidInString(text, out userTestPK))
				{
					this.DialogResult = DialogResult.OK;
					SaveGuid(userTestPK);
					Close();
				}
				else
				{
					Globals.Message.ShowError("Please enter a valid UserTestPK.");
				}
			}
		}

		void LoadPreviousDatTestForm_Activated(object sender, EventArgs e)
		{
#if !WINZOR
			if (SafeClipboard.ContainsText())
			{
				var clipboardText = SafeClipboard.GetText();
				if (clipboardText != null)
				{
					Guid userTestPK;
					if (TryFindGuidInString(clipboardText, out userTestPK))
					{
						UserTestPKTextBox.Text = userTestPK.ToString();
					}
					else
					{
						var oldGuid = LoadGuid();
						if (oldGuid.HasValue)
						{
							UserTestPKTextBox.Text = oldGuid.Value.ToString();
						}
					}
				}
			}
#endif
		}
	}
}
#endif
