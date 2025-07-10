using System;
using System.Windows.Forms;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	public class WriteToLogMenuItem : ZMenuItem
	{
		public WriteToLogMenuItem(IStmALogParent loggedBusinessObject, SecurityCheckpoint security, string caption)
			: this(loggedBusinessObject, delegate { return loggedBusinessObject; }, security, caption)
		{
		}

		public WriteToLogMenuItem(IStmALogParent loggedBusinessObject, SecurityCheckpoint security, MultilingualString caption)
			: this(loggedBusinessObject, delegate { return loggedBusinessObject; }, security, caption)
		{
		}

		public WriteToLogMenuItem(IStmALogParent topLevelBusinessObject, GetLoggedBusinessObjectDelegate getLoggedBusinessObjectDelegate, SecurityCheckpoint security, string caption)
			: this(topLevelBusinessObject, getLoggedBusinessObjectDelegate, security, caption, new BusinessObjectLoggerOptions())
		{
		}

		public WriteToLogMenuItem(IStmALogParent topLevelBusinessObject, GetLoggedBusinessObjectDelegate getLoggedBusinessObjectDelegate, SecurityCheckpoint security, MultilingualString caption)
			: this(topLevelBusinessObject, getLoggedBusinessObjectDelegate, security, caption, new BusinessObjectLoggerOptions())
		{
		}

		public WriteToLogMenuItem(IStmALogParent topLevelBusinessObject, GetLoggedBusinessObjectDelegate getLoggedBusinessObjectDelegate, SecurityCheckpoint security, string fullCaption, BusinessObjectLoggerOptions loggerOptions)
			: this(topLevelBusinessObject, getLoggedBusinessObjectDelegate, security, (NoResString)fullCaption, loggerOptions)
		{ }

		public WriteToLogMenuItem(IStmALogParent topLevelBusinessObject, GetLoggedBusinessObjectDelegate getLoggedBusinessObjectDelegate, SecurityCheckpoint security, MultilingualString fullCaption, BusinessObjectLoggerOptions loggerOptions)
			: base(fullCaption)
		{
			this.security = security;
			this.caption = fullCaption;
			this.topLevelBusinessObject = topLevelBusinessObject;
			this.getLoggedBusinessObjectDelegate = getLoggedBusinessObjectDelegate;
			this.loggerOptions = loggerOptions;

			Click += new EventHandler(WriteToLogMenuItem_Click);
		}

		public delegate IStmALogParent GetLoggedBusinessObjectDelegate();

		public void AddTo(MenuItem parentMenuItem)
		{
			if (isAdded)
			{
				throw new InvalidOperationException("This WriteToLogMenuItem has already been added to a parentMenuItem.");
			}

			if (parentMenuItem.MenuItems.Count > 0)
			{
				if (parentMenuItem.MenuItems[parentMenuItem.MenuItems.Count - 1].Text != "-")
				{
					parentMenuItem.MenuItems.Add("-");
				}
			}

			isAdded = true;
			parentMenuItem.MenuItems.Add(this);
		}

		public void SetLoggedBusinessObject(IStmALogParent loggedBusinessObject)
		{
			if (topLevelBusinessObject == this.LoggedBusinessObject)
			{
				topLevelBusinessObject = loggedBusinessObject;
			}
			this.loggedBusinessObject = loggedBusinessObject;
		}

		public string DefaultReference
		{
			get { return defaultReference; }
			set { defaultReference = value; }
		}

		#region Show Write To Log Form

		void WriteToLogMenuItem_Click(object sender, EventArgs e)
		{
			ShowWriteToLogForm();
		}

		protected void ShowWriteToLogForm()
		{
			WriteToLogFormInvoker.ShowWriteToLogForm(topLevelBusinessObject, LoggedBusinessObject, Text, security, caption, defaultReference, loggerOptions, ShowHasChangesMessage, OnLogged);
		}

		protected internal void OnLoggedInternal(bool writeToLogSucceeded) => OnLogged(writeToLogSucceeded);
		protected virtual void OnLogged(bool writeToLogSucceeded)
		{
			if (Logged != null)
			{
				Logged(this, new LoggedEventArgs(writeToLogSucceeded));
			}
		}

		void ShowHasChangesMessage(string message)
		{
			Globals.Message.ShowInformation(message, Text);
		}

		public event EventHandler<LoggedEventArgs> Logged;

		#endregion

		#region Implementation

		bool isAdded;
		string defaultReference;
		readonly GetLoggedBusinessObjectDelegate getLoggedBusinessObjectDelegate;
#if DEBUG
		internal
#endif
		IStmALogParent topLevelBusinessObject;
#if DEBUG
		internal
#endif
		readonly SecurityCheckpoint security;
#if DEBUG
		internal
#endif
		readonly string caption;
		readonly BusinessObjectLoggerOptions loggerOptions;
#if DEBUG
		internal
#endif
		IStmALogParent LoggedBusinessObject
		{
			get
			{
				if (loggedBusinessObject == null && getLoggedBusinessObjectDelegate != null)
				{
					loggedBusinessObject = getLoggedBusinessObjectDelegate();
				}
				return loggedBusinessObject;
			}
		}
		IStmALogParent loggedBusinessObject;

		#endregion
	}
}
