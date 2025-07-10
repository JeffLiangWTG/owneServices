#if DEBUG

using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Testing
{
	public class TestListLoader : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region MachineName

		public ZString MachineName
		{
			get { return machineName; }
			set
			{
				if (machineName != value)
				{
					CheckMaximumLength(MachineNameInfo, value);
					SetNonPersistentPropertyValue(MachineNameInfo, ref machineName, value);
				}
			}
		}

		ZString machineName;

		public ZPropertyInfo MachineNameInfo
		{
			get { return GetZPropertyInfo(nameof(MachineName)); }
		}

		#endregion

		#region LoginName

		public ZString LoginName
		{
			get { return loginName; }
			set
			{
				if (loginName != value)
				{
					CheckMaximumLength(LoginNameInfo, value);
					SetNonPersistentPropertyValue(LoginNameInfo, ref loginName, value);
					if (MachineName.IsEmpty)
					{
						MachineName = LoginName;
					}
				}
			}
		}

		ZString loginName;

		public ZPropertyInfo LoginNameInfo
		{
			get { return GetZPropertyInfo(nameof(LoginName)); }
		}

		#endregion

		#region FileName

		public ZString FileName
		{
			get { return fileName; }
			set
			{
				if (fileName != value)
				{
					CheckMaximumLength(FileNameInfo, value);
					SetNonPersistentPropertyValue(FileNameInfo, ref fileName, value);
				}
			}
		}

		public void ValidateFileName()
		{
			FileNameInfo.ClearAllNotifications();
			if (!File.Exists(FileName))
			{
				FileNameInfo.AddError("File does not exist");
			}
		}

		ZString fileName;

		public ZPropertyInfo FileNameInfo
		{
			get { return GetZPropertyInfo(nameof(FileName)); }
		}

		#endregion

		#region Data Access

		public TestMethodInfo[] GetTestMethods()
		{
			List<TestMethodInfo> testMethods = new List<TestMethodInfo>();
			using (StreamReader sr = new StreamReader(FileName))
			{
				String line;
				while ((line = sr.ReadLine()) != null)
				{
					string[] segments = line.Split(',');
					if (segments.Length >= 3)
					{
						string assemblyName = segments[0];
						string className = segments[1];
						string methodName = segments[2];
						testMethods.Add(new TestMethodInfo(assemblyName, className, methodName));
					}
				}
			}

			return testMethods.ToArray();
		}

		#endregion

		#region GroupTestsByAssembly
		public ZBool GroupTestsByAssembly
		{
			get { return groupTestsByAssembly; }
			set { SetNonPersistentPropertyValue(GroupTestsByAssemblyInfo, ref groupTestsByAssembly, value); }
		}
		ZBool groupTestsByAssembly;

		public ZPropertyInfo GroupTestsByAssemblyInfo
		{
			get { return GetZPropertyInfo(nameof(GroupTestsByAssembly)); }
		}
		#endregion

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			GroupTestsByAssembly = true;
		}
	}
}
#endif
