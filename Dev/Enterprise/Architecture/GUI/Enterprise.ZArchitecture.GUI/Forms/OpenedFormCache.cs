using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ZArchitecture.GUI
{
	public sealed class OpenedFormCache
	{
		internal OpenedFormCache()
		{
			FormCache = new ConcurrentDictionary<string, Form>(StringComparer.Ordinal);
		}

		public static OpenedFormCache GetInstance()
		{
			return fInstance;
		}

		public int AdditionalFormsCount
		{
			get
			{
				return additionalFormsCount;
			}
			set
			{
				if (value < 0)
				{
					additionalFormsCount = 0;
				}
				else
				{
					additionalFormsCount = value;
				}
			}
		}
		int additionalFormsCount;

		public int Count
		{
			get { return FormCache.Count; }
		}

		public bool OpenFormsExist
		{
			get
			{
				foreach (var cacheEntry in FormCache)
				{
					Form aForm = cacheEntry.Value;
					if (aForm.IsHandleCreated && aForm.Visible)
					{
						return true;
					}
				}

				return false;
			}
		}

		public bool CloseAllCachedForms()
		{
			var dirtyFormList = new ArrayList();
			var cleanFormList = new ArrayList();

			foreach (var cacheEntry in FormCache)
			{
				if (cacheEntry.Value is IDisplayModeAware)
				{
					var form = (IDisplayModeAware)cacheEntry.Value;
					if (form.DisplayMode == ODisplayMode.Edit
						|| form.DisplayMode == ODisplayMode.New)
					{
						dirtyFormList.Add(form);
					}
					else
					{
						cleanFormList.Add(form);
					}
				}
				else
				{
					var form = cacheEntry.Value;
					cleanFormList.Add(form);
				}
			}

			var isCancelled = false;
			foreach (Form form in dirtyFormList)
			{
				if (form.IsHandleCreated)
				{
					form.Close();
					if (form.IsHandleCreated)
					{
						isCancelled = true;
						break;
					}
				}
			}

			if (!isCancelled)
			{
				foreach (Form form in cleanFormList)
				{
					if (form.IsHandleCreated)
					{
						try
						{
							form.Close();
						}
						catch (InvalidOperationException)
						{
						}
					}
				}
			}
			return !isCancelled;
		}

		public bool Contains(Guid masterRecordPK, string moduleName)
		{
			return FormCache.ContainsKey(GetKey(masterRecordPK, moduleName));
		}

		public Form GetForm(Guid masterRecordPK, string moduleName)
		{
			Form result = null;
			FormCache.TryGetValue(GetKey(masterRecordPK, moduleName), out result);
			return result;
		}

		public void SwitchToCachedForm(Guid masterRecordPK, string moduleName)
		{
			var cachedForm = FormCache[GetKey(masterRecordPK, moduleName)];
			if (cachedForm is ZForm form)
			{
				form.RestoreLastWindowStateFromMinimized();
			}
			else if (cachedForm.WindowState == FormWindowState.Minimized)
			{
				cachedForm.WindowState = FormWindowState.Normal;
			}

			cachedForm.BringToFront();
			cachedForm.Activate();
		}

		public void Add(Guid masterRecordPK, Form formToCache, string moduleName)
		{
			if (formToCache != null)
			{
				if (!FormCache.TryAdd(GetKey(masterRecordPK, moduleName), formToCache))
				{
					throw new ArgumentException("An element with the same key already exists in the ConcurrentDictionary<string, Form>.");
				}
				formToCache.Disposed += new EventHandler(FlushFromCache);
			}
		}

#if WINZOR

		public void Remove(Guid masterRecordPk, string moduleName)
		{
			FormCache.TryRemove(GetKey(masterRecordPk, moduleName), out _);
		}

#endif

		internal ICollection<Form> GetAllOpenForms()
		{
			return FormCache.Values;
		}

		internal void SaveAllFormsPositionAndSize()
		{
			foreach (var form in GetAllOpenForms().OfType<ZForm>())
			{
				try
				{
					if (form.ShouldRememberPositionAndSize)
					{
						EnterpriseFormLookStrategy.SavePositionAndSize(form);
					}
				}
				catch (Exception e)
				{
					ErrorReporter.ReportOnce("ExceptionInSaveAllFormsPositionAndSize", e);
				}
			}
		}

		public static WeakReference LastActiveForm
		{
			get
			{
				if (lastActiveForm == null)
				{
					lastActiveForm = new WeakReference(null);
				}

				return lastActiveForm;
			}
		}

		[ThreadStatic]
		static WeakReference lastActiveForm;

		#region Implementation

		[ThreadSafe]
		static readonly OpenedFormCache fInstance = new OpenedFormCache();

		public ConcurrentBag<Form> deliberatelyLeakedForms;
		public bool IsDeliberatelyLeakingForms { get; set; }

#if DEBUG
		[CargoWise.Common.Testing.SuppressWeaklyTypedCollectionMessage]
		public
#endif
		ConcurrentDictionary<string, Form> FormCache;

		[SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccess")]
		void FlushFromCache(object sender, EventArgs e)
		{
			foreach (var cacheEntry in FormCache)
			{
				if (cacheEntry.Value == sender)
				{
					Form removedValue;
					FormCache.TryRemove(cacheEntry.Key, out removedValue);

					if (IsDeliberatelyLeakingForms)
					{
						deliberatelyLeakedForms.Add(removedValue);
					}

					return;
				}
			}
		}

		string GetKey(Guid pK, string moduleName)
		{
			return pK.ToString() + moduleName;
		}

		#endregion
	}
}
