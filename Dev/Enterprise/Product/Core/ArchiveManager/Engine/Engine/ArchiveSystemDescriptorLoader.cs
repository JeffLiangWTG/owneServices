using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.ArchiveManager.Engine
{
	/// <summary>
	/// Loads ArchiveSystemDescriptor objects across the entire system.
	/// </summary>
	public class ArchiveSystemDescriptorLoader : IArchiveSystemDescriptorLoader
	{
		public IEnumerable<IArchiveSystemDescriptor> Load()
		{
			var descriptorList = new List<IArchiveSystemDescriptor>();
			var archiveAttributes = AssemblyMetaDataReader.GetAttributes<ArchiveSystemDescriptorProviderAttribute>().ToArray();

			foreach (var attribute in archiveAttributes)
			{
				if (attribute is ArchiveSystemDescriptorProviderAttribute descriptorAttribute)
				{
					var descriptor = LoadFromAttribute(descriptorAttribute);
					if (descriptor != null)
					{
						descriptorList.Add(descriptor);
					}
				}
			}

			if (!SystemDataRegistry.Instance.ExposeExpiredRatesArchiveSystem.Value)
			{
				descriptorList.RemoveAll(descriptor => descriptor.Code == "RED");
			}

			if (!SystemDataRegistry.Instance.ExposeActivityLogsArchiveSystem.Value)
			{
				descriptorList.RemoveAll(descriptor => descriptor.Code == "PAL");
			}

			return descriptorList;
		}

		IArchiveSystemDescriptor LoadFromAttribute(ArchiveSystemDescriptorProviderAttribute attribute)
		{
			IArchiveSystemDescriptor descriptor = null;
			var descriptorType = attribute.Type;
			if (typeof(IArchiveSystemDescriptor).IsAssignableFrom(descriptorType))
			{
				try
				{
					descriptor = (IArchiveSystemDescriptor)Activator.CreateInstance(descriptorType);
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					descriptor = null;
					ErrorReporter.ReportOnce("Error loading ArchiveSystemDescriptor type: ", e);
				}
			}
			else
			{
				ErrorReporter.ReportOnce($"Type provided: {descriptorType.ToString()} is not an IArchiveSystemDescriptor");
			}

			return descriptor;
		}
	}
}
