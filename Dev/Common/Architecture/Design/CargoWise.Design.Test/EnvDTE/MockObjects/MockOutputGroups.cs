using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace CargoWise.Design.DTE.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class MockOutputGroups : List<MockOutputGroup>, EnvDTE.OutputGroups
	{
		public EnvDTE.OutputGroup Item(object index)
		{
			foreach (MockOutputGroup outputGroup in this)
			{
				if (outputGroup.CanonicalName == index as string ||
					outputGroup.DisplayName == index as string)
				{
					return outputGroup;
				}
			}
			return null;
		}

		IEnumerator EnvDTE.OutputGroups.GetEnumerator()
		{
			foreach (MockOutputGroup outputGroup in this)
			{
				yield return outputGroup;
			}
		}

		#region OutputGroups Unsupported Members

		public EnvDTE.DTE DTE
		{ get { throw new NotSupportedException(); } }

		public EnvDTE.Configuration Parent
		{ get { throw new NotSupportedException(); } }

		#endregion
	}
}
