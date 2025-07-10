using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZClientScriptManager
	{
		public ZClientScriptManager(ZPage page)
		{
			this.Page = page;
			page.PreRenderComplete += new EventHandler(RegisterScripts);
		}

		void RegisterScripts(object sender, EventArgs e)
		{
			RenderingComplete = true;
			RegisterClientForEventScriptBlocks();
		}
		bool RenderingComplete;

		#region WebResources

		public string GetWebResourceUrl(Type type, string resourceName)
		{
			string result = "";
#if DEBUG
			// This is for testing purposes only as we do not have a runtime or configuration to use
			if (Globals.IsTest)
			{
				result = "/WebResource.axd?d=SbXSD3uTnhYsK4gMD8fL84_mHPC5jJ7lfdnr1_WtsftZiUOZ6IXYG8QCXW86UizF0&t=632768953157700078";
			}
			else
#endif
			{
				result = Page.ClientScriptInternal.GetWebResourceUrl(type, resourceName);
			}
			return result;
		}
		#endregion

		#region ClientScriptBlock

		public bool IsClientScriptBlockRegistered(ZString key)
		{
			return Page.ClientScriptInternal.IsClientScriptBlockRegistered(key);
		}

		public bool IsClientScriptBlockRegistered(Type type, ZString key)
		{
			return Page.ClientScriptInternal.IsClientScriptBlockRegistered(type, key);
		}

		public void RegisterClientScriptBlock(Type type, ZString key, ZString script)
		{
			Regex pattern = new Regex("^<SCRIPT[^>]*(FOR=.*EVENT=.*)|(EVENT=.*FOR=.*)>", RegexOptions.IgnoreCase);
			if (pattern.IsMatch(script))
			{
				ErrorReporter.ReportOnce("RegisterStartupScript should not be used to register scripts containing FOR=object EVENT=event. Duplicates are not handled correctly. Use RegisterForEventScriptBlock instead");
			}

			Page.ClientScriptInternal.RegisterClientScriptBlock(type, key, script);
		}

		public void RegisterClientScriptBlock(Type type, ZString key, ZString script, bool addScriptTags)
		{
			Regex pattern = new Regex("^<SCRIPT[^>]*(FOR=.*EVENT=.*)|(EVENT=.*FOR=.*)>", RegexOptions.IgnoreCase);
			if (pattern.IsMatch(script))
			{
				ErrorReporter.ReportOnce("RegisterStartupScript should not be used to register scripts containing FOR=object EVENT=event. Duplicates are not handled correctly. Use RegisterForEventScriptBlock instead");
			}

			Page.ClientScriptInternal.RegisterClientScriptBlock(type, key, script, addScriptTags);
		}
		#endregion

		#region ClientScriptInclude

		public virtual bool IsClientScriptIncludeRegistered(ZString key)
		{
			return Page.ClientScriptInternal.IsClientScriptIncludeRegistered(key);
		}

		public virtual void RegisterClientScriptInclude(ZString key, ZString scriptUrl)
		{
			Page.ClientScriptInternal.RegisterClientScriptInclude(key, scriptUrl);
		}

		#endregion

		#region StartupScript

		public bool IsStartupScriptRegistered(ZString key)
		{
			return Page.ClientScriptInternal.IsStartupScriptRegistered(key);
		}

		public bool IsStartupScriptRegistered(Type type, ZString key)
		{
			return Page.ClientScriptInternal.IsStartupScriptRegistered(type, key);
		}

		public void RegisterStartupScript(Type type, ZString key, ZString script)
		{
			Regex pattern = new Regex("^<SCRIPT[^>]*(FOR=.*EVENT=.*)|(EVENT=.*FOR=.*)>", RegexOptions.IgnoreCase);
			if (pattern.IsMatch(script))
			{
				ErrorReporter.ReportOnce("RegisterStartupScript should not be used to register scripts containing FOR=object EVENT=event. Duplicates are not handled correctly. Use RegisterForEventScriptBlock instead");
			}

			Page.ClientScriptInternal.RegisterStartupScript(type, key, script);
		}

		public void RegisterStartupScript(Type type, ZString key, ZString script, bool addScriptTags)
		{
			Regex pattern = new Regex("^<SCRIPT[^>]*(FOR=.*EVENT=.*)|(EVENT=.*FOR=.*)>", RegexOptions.IgnoreCase);
			if (pattern.IsMatch(script))
			{
				ErrorReporter.ReportOnce("RegisterStartupScript should not be used to register scripts containing FOR=object EVENT=event. Duplicates are not handled correctly. Use RegisterForEventScriptBlock instead");
			}

			Page.ClientScriptInternal.RegisterStartupScript(type, key, script, addScriptTags);
		}
		#endregion

		#region RegisterClientForEventScriptBlock

		public bool IsClientForEventScriptBlockRegistered(ZString scriptObject, ZString scriptEvent, ZString key)
		{
			return IsClientForEventScriptBlockRegistered(scriptObject, scriptEvent, GetType(), key);
		}

		public bool IsClientForEventScriptBlockRegistered(ZString scriptObject, ZString scriptEvent, Type type, ZString key)
		{
			ForEventKey realKey = new ForEventKey(scriptObject, scriptEvent);
			return RegisteredForEventScripts.ContainsKey(realKey) && RegisteredForEventScripts[realKey].Contains(key);
		}

		public void RegisterClientForEventScriptBlock(ZString scriptObject, ZString scriptEvent, Type type, ZString scriptKey, ZString script)
		{
			if (RenderingComplete)
			{
				throw new InvalidOperationException("Attempted to register script block after rendering has been completed");
			}

			ForEventKey realKey = new ForEventKey(scriptObject, scriptEvent);
			ZString existingScript;
			if (!RegisteredForEventScripts.ContainsKey(realKey) || !RegisteredForEventScripts[realKey].Contains(scriptKey))
			{
				Regex scriptPattern = new Regex(@"^\s*<SCRIPT[^>]*>|</SCRIPT>\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline);
				Regex endsWithSemiColon = new Regex(@"\s*;\s*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
				scriptPattern.Replace(script, "");
				if (!endsWithSemiColon.IsMatch(script))
				{
					script += ";";
				}

				if (ClientForEventScripts.TryGetValue(realKey, out existingScript))
				{
					existingScript += script + "\n";
				}
				else
				{
					existingScript = script + "\n";
				}
				ClientForEventScripts[realKey] = existingScript;

				StringCollection scriptKeys;
				if (!RegisteredForEventScripts.TryGetValue(realKey, out scriptKeys))
				{
					scriptKeys = new StringCollection();
				}
				scriptKeys.Add(scriptKey);
				RegisteredForEventScripts[realKey] = scriptKeys;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1117:DuplicateForEventScripts", Justification = "This is the only point where the registered ForEvent scripts are actually registered.")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is the only point where the registered ForEvent scripts are actually registered.")]
		void RegisterClientForEventScriptBlocks()
		{
			foreach (KeyValuePair<ForEventKey, ZString> entry in ClientForEventScripts)
			{
				if (!entry.Value.IsEmpty)
				{
					ZString script = entry.Value;
					if (!entry.Key.For.IsEmpty && !entry.Key.Event.IsEmpty)
					{
						script = ZString.Format("<SCRIPT FOR={0} EVENT={1}>\n{2}\n</SCRIPT>\n", entry.Key.For, entry.Key.Event, script);
					}
					Page.ClientScriptInternal.RegisterClientScriptBlock(Page.GetType(), String.Format("{0}_{1}", entry.Key.For, entry.Key.Event), script);
				}
			}
		}

		readonly Dictionary<ForEventKey, ZString> ClientForEventScripts = new Dictionary<ForEventKey, ZString>();
		readonly Dictionary<ForEventKey, StringCollection> RegisteredForEventScripts = new Dictionary<ForEventKey, StringCollection>();
		#endregion

		#region Helper Methods for Testing
#if DEBUG
		public ZString GetForEventScriptForTesting(ZString scriptObject, ZString scriptEvent)
		{
			ForEventKey realKey = new ForEventKey(scriptObject, scriptEvent);
			ZString result;
			ClientForEventScripts.TryGetValue(realKey, out result);
			return result;
		}

		public StringCollection GetRegisteredForEventKeysForTesting(ZString scriptObject, ZString scriptEvent)
		{
			ForEventKey realKey = new ForEventKey(scriptObject, scriptEvent);
			StringCollection result;
			RegisteredForEventScripts.TryGetValue(realKey, out result);
			return result;
		}
#endif
		#endregion

		#region HiddenFields

		public virtual void RegisterHiddenField(ZString name, ZString value)
		{
			Page.ClientScriptInternal.RegisterHiddenField(name, value);
		}

		#endregion

		protected readonly ZPage Page;
	}

	#region EventKey

	public class ForEventKey
	{
		public ForEventKey(ZString @for, ZString @event)
		{
			this.For = @for.ToLower();
			this.Event = @event.ToLower();
		}
		public readonly ZString For;
		public readonly ZString Event;

		public override bool Equals(object obj)
		{
			bool res = false;
			ForEventKey key = obj as ForEventKey;
			if (key != null)
			{
				res = (key.Event == Event && key.For == For);
			}
			return res;
		}

		public override int GetHashCode()
		{
			return For.GetHashCode() ^ Event.GetHashCode();
		}
	}

	#endregion EventKey
}
