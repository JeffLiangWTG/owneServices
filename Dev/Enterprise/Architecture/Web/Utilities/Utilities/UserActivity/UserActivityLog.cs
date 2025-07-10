using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Web;
using CargoWise.Common;

namespace Enterprise.ZArchitecture.Web.Utilities
{
	public class UserActivityLog
	{
		#region Constants

		public const string PageInitCall = "PageInitCall";
		public const string PageLoadCall = "PageLoadCall";
		public const string PageBindCall = "PageBindCall";
		public const string PageRenderCall = "PageRenderCall";

		#endregion

		#region Constructors

		[SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "IIS time, not PC")]
		public UserActivityLog()
		{
			this.createdDateTime = DateTime.Now; // IIS time, not PC
			requestParameters = new NameValueCollection();
			if (HttpContext.Current != null && HttpContext.Current.Request != null)
			{
				foreach (string key in HttpContext.Current.Request.Params.Keys)
				{
					try
					{
						requestParameters.Add(key, HttpContext.Current.Request.Params[key]);
					}
					catch (Exception e) when (!e.IsCriticalException()) { }
				}
			}

			actionTimes = new Dictionary<string, DateTime>();
			actionOccurrences = new Dictionary<string, long>();
		}

		#endregion

		#region Properties

		public DateTime CreatedDateTime
		{
			get
			{
				return this.createdDateTime;
			}
		}

		public Dictionary<string, DateTime> ActionTimes
		{
			get
			{
				return this.actionTimes;
			}
		}

		public Dictionary<string, long> ActionOccurrences
		{
			get
			{
				return this.actionOccurrences;
			}
		}

		public NameValueCollection RequestParameters
		{
			get
			{
				return this.requestParameters;
			}
		}

		#endregion

		#region Methods

		[SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "IIS time, not PC")]
		public void LogActionTime(string actionKey)
		{
			if (actionTimes.ContainsKey(actionKey))
			{
				actionTimes[actionKey] = DateTime.Now; // IIS time, not PC
			}
			else
			{
				actionTimes.Add(actionKey, DateTime.Now); // IIS time, not PC
			}
			if (actionOccurrences.ContainsKey(actionKey))
			{
				actionOccurrences[actionKey] = actionOccurrences[actionKey]++;
			}
			else
			{
				actionOccurrences[actionKey] = 1;
			}
		}

		#endregion

		#region Implementation

		readonly Dictionary<string, DateTime> actionTimes;
		readonly Dictionary<string, long> actionOccurrences;
		readonly NameValueCollection requestParameters;
		readonly DateTime createdDateTime;

		#endregion
	}
}
