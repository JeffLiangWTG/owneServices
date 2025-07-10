using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.ProcessManagement.Business;

namespace Enterprise.Client.EDI.IssueManager.Business
{
	public class ExceptionKeyFields : AutoHelpErrorLog.Schema
	{
		public ExceptionKeyFields()
		{
			messages = new List<ZString>();
		}

		public ZString Key
		{
			get { return key; }
			set
			{
				key = value.Trim();
				logKey = null;
			}
		}

		public ZString Type
		{
			get { return type; }
			set
			{
				type = value.Left(HE_ExceptionTypeMaxLength).Trim();
				logKey = null;
			}
		}

		public ZString Source
		{
			get { return source; }
			set
			{
				source = value.Left(HE_ExceptionSourceMaxLength).Trim();
				logKey = null;
			}
		}

		public ZString Message
		{
			get { return messages.Count > 0 ? messages[messages.Count - 1] : ZString.Empty; }
			set
			{
				messages.Clear();
				AddMessage(value);
			}
		}

		public void AddMessage(ZString m)
		{
			if (!m.IsEmpty)
			{
				ZString value = m.Trim().Left(HE_ExceptionMessageMaxLength);
				if (value.Length > 0)
				{
					messages.Add(value);
					logKey = null;
				}
			}
		}

		public ReadOnlyCollection<ZString> Messages
		{
			get { return messages.AsReadOnly(); }
		}

		public ZString CallStack
		{
			get { return callStack; }
			set
			{
				callStack = value.Trim();
				var stackDepthLimit = EDIDataRegistry.Instance.ExceptionKeyStacktraceDepths.Value[Type];
				if (stackDepthLimit != null)
				{
					callStack = string.Join(System.Environment.NewLine, callStack.ToString()
						.Split(new[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
						.Take(stackDepthLimit.StackDepth));
				}
				logKey = null;
			}
		}

		public ZBool IsClientVisible
		{
			get { return isClientVisible; }
			set { isClientVisible = value; }
		}

		public string LogKey
		{
			get
			{
				if (logKey == null)
				{
					logKey = KeyBuilder.BuildKey(this);
				}
				return logKey;
			}
		}

		ExceptionKeyBuilder KeyBuilder
		{
			get { return keyBuilder ?? (keyBuilder = new ExceptionKeyBuilder()); }
		}

		public void CopyToLog(EdiHelpErrorLog log)
		{
			var errorLogKey = log.Factory.New<HelpErrorLogKey>();
			errorLogKey.HK_HE = log.PK;
			errorLogKey.HK_Key = LogKey;
			errorLogKey.HK_HashCode = HelpErrorLogKey.GetKeyHashCode(errorLogKey.HK_Key);

			log.HE_ExceptionType = Type;
			log.HE_ExceptionSource = Source;
			log.HE_ExceptionMessage = BuildReadableMessage();
			log.HE_IsClientVisible = IsClientVisible;
		}

		ZString BuildReadableMessage()
		{
			const string ConcurrencyError = "ConcurrencyError";
			const string TableName = "TABLENAME";
			var exceptionMessage = Message;
			exceptionMessage = exceptionMessage.IsWesternEuropeanOrEmpty ? exceptionMessage : ConvertUnknownCharacters(exceptionMessage);
			int index = exceptionMessage.IndexOf(ConcurrencyError, StringComparison.Ordinal);
			if (index >= 0 && exceptionMessage.Length <= ConcurrencyError.Length + 4)
			{
				ZString outerMessage = this.Messages[0].Replace("\r", " ").Replace("\n", " ");
				exceptionMessage = ConcurrencyError + ' ' + Regex.Match(outerMessage, @"(?i)(?<=\b" + TableName + @":\s*)(\w+)").Groups[1].Value;
			}
			return exceptionMessage;
		}
		static ZString ConvertUnknownCharacters(ZString s)
		{
			var sb = new ZStringBuilder();
			foreach (char ch in s)
			{
				if (char.IsControl(ch))
				{
					sb.Append("?");
				}
				else
				{
					sb.Append(ch.ToString());
				}
			}
			return sb.ToString();
		}

		public EdiHelpErrorLog MatchingLog(IHelpErrorLogCollection logs)
		{
			return logs.Find(LogKey);
		}

		#region Implementation

		ZString key;
		ZString type;
		ZString source;
		readonly List<ZString> messages;
		ZString callStack;
		ZBool isClientVisible;
		ExceptionKeyBuilder keyBuilder;
		string logKey;

		#endregion
	}
}

