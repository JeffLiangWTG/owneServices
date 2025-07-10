using System;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Web.UI;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	[HttpContextEnabledTest]
	public abstract class ZPageLifeCycleTest : TestCaseWithFactory
	{
		#region Methods

		protected abstract ZPage GetNewPage();

		protected virtual ZPage TestPage
		{
			get
			{
				if (fTestPage == null)
				{
					fTestPage = GetNewPage();
				}
				return fTestPage;
			}
		}
		ZPage fTestPage;

		protected virtual NameValueCollection PostData
		{
			get
			{
				if (fPostData == null)
				{
					fPostData = new NameValueCollection();
				}
				return fPostData;
			}
		}
		NameValueCollection fPostData;

		protected virtual object ViewState { get; set; }

		protected virtual NameValueCollection GetLeftOverPostData()
		{
			return new NameValueCollection();
		}
		#endregion

		protected bool IsPostBack;
		protected virtual bool NeedRenderControl
		{
			get { return true; }
		}

		#region Test Methods

		protected void RunPageLifeCycle()
		{
			using (TempDirectory temp = new TempDirectory())
			{
				TestPage.SetServerMappedPathForTest(temp.DirectoryName);
				InvokePrivateMethod(TestPage, typeof(Page), "InitRecursive", new Type[] { typeof(Control) }, new object[] { null });
				if (AssertInitRecursive != null)
				{
					AssertInitRecursive(this, EventArgs.Empty);
				}

				InvokePrivateMethod(TestPage, typeof(Page), "OnInitComplete", new Type[] { typeof(EventArgs) }, new object[] { EventArgs.Empty });
				if (AssertOnInitComplete != null)
				{
					AssertOnInitComplete(this, EventArgs.Empty);
				}

				if (IsPostBack)
				{
					InvokePrivateMethod(TestPage, typeof(Page), "LoadViewState", new Type[] { typeof(object) }, new object[] { ViewState });
					if (AssertLoadViewState != null)
					{
						AssertLoadViewState(this, EventArgs.Empty);
					}

					InvokePrivateMethod(TestPage, typeof(Page), "ProcessPostData", new Type[] { typeof(NameValueCollection), typeof(bool) }, new object[] { PostData, true });
					if (AssertProcessPostData != null)
					{
						AssertProcessPostData(this, EventArgs.Empty);
					}
				}

				InvokePrivateMethod(TestPage, typeof(Page), "OnPreLoad", new Type[] { typeof(EventArgs) }, new object[] { EventArgs.Empty });
				if (AssertOnPreLoad != null)
				{
					AssertOnPreLoad(this, EventArgs.Empty);
				}

				InvokePrivateMethod(TestPage, typeof(Page), "LoadRecursive", Array.Empty<Type>(), null);
				if (AssertLoadRecursive != null)
				{
					AssertLoadRecursive(this, EventArgs.Empty);
				}

				if (IsPostBack)
				{
					InvokePrivateMethod(TestPage, typeof(Page), "ProcessPostData", new Type[] { typeof(NameValueCollection), typeof(bool) }, new object[] { PostData, false });
					if (AssertProcessLeftOverPostData != null)
					{
						AssertProcessLeftOverPostData(this, EventArgs.Empty);
					}

					InvokePrivateMethod(TestPage, typeof(Page), "RaiseChangedEvents", Array.Empty<Type>(), null);
					if (AssertRaiseChangedEvents != null)
					{
						AssertRaiseChangedEvents(this, EventArgs.Empty);
					}

					InvokePrivateMethod(TestPage, typeof(Page), "RaisePostBackEvent", new Type[] { typeof(NameValueCollection) }, new object[] { PostData });
					if (AssertRaisePostBackEvent != null)
					{
						AssertRaisePostBackEvent(this, EventArgs.Empty);
					}
				}

				InvokePrivateMethod(TestPage, typeof(Page), "OnLoadComplete", new Type[] { typeof(EventArgs) }, new object[] { EventArgs.Empty });
				if (AssertOnLoadComplete != null)
				{
					AssertOnLoadComplete(this, EventArgs.Empty);
				}

				InvokePrivateMethod(TestPage, typeof(Page), "PreRenderRecursiveInternal", Array.Empty<Type>(), null);
				if (AssertPreRenderRecursiveInternal != null)
				{
					AssertPreRenderRecursiveInternal(this, EventArgs.Empty);
				}

				InvokePrivateMethod(TestPage, typeof(Page), "PerformPreRenderComplete", Array.Empty<Type>(), null);
				if (AssertPerformPreRenderComplete != null)
				{
					AssertPerformPreRenderComplete(this, EventArgs.Empty);
				}

				ViewState = InvokePrivateMethod(TestPage, typeof(Page), "SaveViewState", Array.Empty<Type>(), null);
				if (AssertSaveViewState != null)
				{
					AssertSaveViewState(this, new SaveViewStateEventArgs(ViewState));
				}

				if (NeedRenderControl)
				{
					StringWriter output = new StringWriter();
					HtmlTextWriter writer = new HtmlTextWriter(output);
					TestPage.RenderControl(writer);

					if (AssertRenderComplete != null)
					{
						AssertRenderComplete(this, new RenderCompleteEventArgs(new ZString(output.ToString())));
					}
				}

				InvokePrivateMethod(TestPage, typeof(Page), "UnloadRecursive", new Type[] { typeof(bool) }, new object[] { true });
				if (AssertUnload != null)
				{
					AssertUnload(this, EventArgs.Empty);
				}

				fTestPage = null;
			}
		}

		#endregion

		#region EventHandlers

		public event EventHandler AssertInitRecursive;
		public event EventHandler AssertOnInitComplete;
		public event EventHandler AssertLoadViewState;
		public event EventHandler AssertProcessPostData;
		public event EventHandler AssertOnPreLoad;
		public event EventHandler AssertLoadRecursive;
		public event EventHandler AssertProcessLeftOverPostData;
		public event EventHandler AssertRaiseChangedEvents;
		public event EventHandler AssertRaisePostBackEvent;
		public event EventHandler AssertOnLoadComplete;
		public event EventHandler AssertPreRenderRecursiveInternal;
		public event EventHandler AssertPerformPreRenderComplete;
		public event RenderCompleteEventHandler AssertRenderComplete;
		public event SaveViewStateEventHander AssertSaveViewState;
		public event EventHandler AssertUnload;

		public delegate void RenderCompleteEventHandler(object sender, RenderCompleteEventArgs e);
		public delegate void SaveViewStateEventHander(object sender, SaveViewStateEventArgs e);

		#endregion
		#region Helper Methods

		object InvokePrivateMethod(object invokeObject, Type invokeType, string method1, Type[] argTypes, object[] args)
		{
			MethodInfo method = invokeType.GetMethod(method1, BindingFlags.Instance | BindingFlags.NonPublic, null, argTypes, null);
			AssertNotNull(String.Format("Failed to get MethodInfo for {0}", method1), method);
			return method.Invoke(invokeObject, args);
		}
		#endregion
	}
}
