using System;
using System.Collections;
using System.ComponentModel;

namespace CargoWise.Design.DTE.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class MockOutputGroup : MarshalByRefObject, EnvDTE.OutputGroup
	{
		public void AddFileUrl(string url)
		{ fileUrLs.Add(url); }

		#region OutputGroup Members

		public object FileURLs
		{ get { return fileUrLs; } }

		readonly ArrayList fileUrLs = new ArrayList();

		string EnvDTE.OutputGroup.CanonicalName
		{ get { return CanonicalName; } }

		public string CanonicalName
		{
			get { return canonicalName; }
			set { canonicalName = value; }
		}
		string canonicalName;

		string EnvDTE.OutputGroup.DisplayName
		{ get { return DisplayName; } }

		public string DisplayName
		{
			get { return displayName; }
			set { displayName = value; }
		}
		string displayName;

		#region Not Supported

		public EnvDTE.DTE DTE
		{ get { throw new NotSupportedException(); } }

		public string Description
		{ get { throw new NotSupportedException(); } }

		public object FileNames
		{ get { throw new NotSupportedException(); } }

		public EnvDTE.OutputGroups Collection
		{ get { throw new NotSupportedException(); } }

		public int FileCount
		{ get { throw new NotSupportedException(); } }

		#endregion

		#endregion
	}
}
