namespace Enterprise.AuditDataServices.UatRunner
{
	using System;
	using System.Windows.Forms;
	using CargoWise.Data;

	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1076:MessageBoxShow", Justification = "Development only project (UatRunner).")]
		static void Main(string[] args)
		{
			try
			{
				InitaliseDatabase(args);

				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new AuditSubscriptionForm());
			}
			catch (ArgumentException ex)
			{
				MessageBox.Show(ex.Message); // Development only project (UatRunner).
			}
		}

		static void InitaliseDatabase(string[] args)
		{
			if (args.Length != 2)
			{
				throw new ArgumentException("Must specify server and database names.", nameof(args));
			}

			Db.InitializeDatabaseDetails(args[0].Trim(), args[1].Trim());
		}
	}
}
