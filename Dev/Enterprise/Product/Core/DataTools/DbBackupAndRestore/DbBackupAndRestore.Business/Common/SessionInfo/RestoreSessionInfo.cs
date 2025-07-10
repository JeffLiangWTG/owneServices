using System;
using System.Security.Cryptography;
using System.Text;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	public abstract class SessionInfo
	{
		public SessionInfo(string dbServer, string databaseName)
		{
			this.dbServer = dbServer;
			this.databaseName = databaseName;
		}

		#region Properties

		public string DbServer
		{
			get { return dbServer; }
		}

		readonly string dbServer;

		public string DatabaseName
		{
			get { return databaseName; }
		}

		readonly string databaseName;

		public string SessionDisplay
		{
			get { return sessionDisplay; }
		}

		protected string sessionDisplay;

		#endregion

		protected string GetReleaseKey(string sessionIdHash)
		{
			string source = String.Format(
				"TheDbName:{0};TheSession:{1};TheDbServer:{2};",
				databaseName.ToUpper(),
				sessionIdHash.ToLower(),
				dbServer.ToLower()
			);

			return GetBase64HashFromString(source);
		}

		protected string GetBase64HashFromString(string source)
		{
			byte[] sourceBytes = Encoding.ASCII.GetBytes(source);
			using var hash = SHA1.Create();
			byte[] hashValue = hash.ComputeHash(sourceBytes);
			string result = Convert.ToBase64String(hashValue);

			return result;
		}

		protected string GetSessionDisplayByMergingExtraCharactersToIdHash(string sessionIdHash, string extraChars)
		{
			int numOfExtraChars = extraChars.Length;
			StringBuilder sessionDisplayBuilder = new StringBuilder(sessionIdHash.Length * 2);

			for (int i = 0; i < sessionIdHash.Length; i++)
			{
				sessionDisplayBuilder.Append(sessionIdHash[i]);
				int charIndex = i % numOfExtraChars;
				sessionDisplayBuilder.Append(extraChars[charIndex]);
			}

			return sessionDisplayBuilder.ToString();
		}

		protected string GetSessionIdHashByUsingOnlyEvenCharactersFromDisplay()
		{
			StringBuilder sessionIdHashBuilder = new StringBuilder();

			for (int i = 0; i < sessionDisplay.Length; i += 2)
			{
				sessionIdHashBuilder.Append(sessionDisplay[i]);
			}

			return sessionIdHashBuilder.ToString();
		}
	}
}
