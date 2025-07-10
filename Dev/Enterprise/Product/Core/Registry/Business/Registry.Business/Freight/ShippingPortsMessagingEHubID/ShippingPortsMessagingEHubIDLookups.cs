using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.Business
{
	public class ShippingPortsMessagingEHubIDLookups : ZLookups
	{
		public ShippingPortsMessagingEHubIDLookups(ShippingPortsMessagingEHubID parent, BusinessObjectFactory currentFactory) : base(parent)
		{
			this.currentFactory = Argument.NotNull(currentFactory, nameof(currentFactory));
		}

		readonly BusinessObjectFactory currentFactory;

		public IBusinessObjectCollection PortList
		{
			get
			{
				if (portList == null)
				{
					var filter = new ZQuery();
					filter.AddToFilter(RefUNLOCOSchema.RL_Code, EnabledPorts.Append(ExperimentalPorts.ToArray()));

					var assembly = Assembly.Load("Enterprise.MasterFiles.Business");
					var refUNLOCOCollectionType = assembly.GetType("Enterprise.MasterFiles.Business.RefUNLOCOCollection");
					portList = (IBusinessObjectCollection)Activator.CreateInstance(refUNLOCOCollectionType, new object[] { currentFactory, filter });
					portList.ApplySort(new SortInfo(RefUNLOCOSchema.Constants.RL_Code, ListSortDirection.Ascending));
				}

				return portList;
			}
		}
		IBusinessObjectCollection portList;

		IEnumerable<string> EnabledPorts
		{
			get
			{
				return enabledPorts ?? (enabledPorts = new[]
				{
					"ESBCN",
					"ESGAN",
					"ESPDS",
					"ESVLC"
				});
			}
		}
		IEnumerable<string> enabledPorts;

		IEnumerable<string> ExperimentalPorts
		{
			get
			{
				return experimentalPorts ?? (experimentalPorts = new[]
				{
					"AUBNE",
					"AUMEL",
					"AUSYD",
					"NZAKL",
					"NZLYT",
					"NZNPE",
					"NZPOE",
					"NZTRG",
					"NZWLG",
					"NZORR"
				});
			}
		}
		IEnumerable<string> experimentalPorts;

		public CodeDescriptionPairList ModuleTypeList
		{
			get
			{
				if (moduleTypeList == null)
				{
					moduleTypeList = new ModuleTypes();
				}

				return moduleTypeList;
			}
		}
		CodeDescriptionPairList moduleTypeList;
	}
}
