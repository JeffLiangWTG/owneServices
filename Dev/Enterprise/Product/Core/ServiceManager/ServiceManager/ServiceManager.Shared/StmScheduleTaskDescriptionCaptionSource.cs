using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.ResourceStrings.Cache;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.ServiceManager.Shared
{
	public class StmScheduleTaskDescriptionCaptionSource : ICustomizableDataCaptionSource
	{
		public ushort Asmid
		{
			get
			{
				return HostedServiceAttribute.HostedServiceDescriptionAssemblyId;
			}
			set
			{
			}
		}

		public string Description
		{
			get
			{
				return ResString.GetMultilingualString("1A4CB4BB-A0DE-403A-BF36-514962D9C9FC", "Service Task Description");
			}
		}

		public int MaxLength
		{
			get { return 0; }
		}

		public IEnumerable<IResString> GetCompileTimeSystemCaptions()
		{
			return AssemblyMetaDataReader.GetAttributes<HostedServiceAttribute>()
				.Select(attr => CustomizableDataResourceStrings.GetMultilingualString(this, null, attr.Description));
		}

		public string GetKey(object context, string caption)
		{
			return Convert.ToBase64String(Encoding.UTF8.GetBytes(caption));
		}

		public IEnumerable<IResString> GetRuntimeCaptions(IResString? userCaption = null, object? context = null)
		{
			return AssemblyMetaDataReader.GetAttributes<HostedServiceAttribute>()
				   .Select(attr => CustomizableDataResourceStrings.GetMultilingualString(this, null, attr.Description));
		}
	}
}
