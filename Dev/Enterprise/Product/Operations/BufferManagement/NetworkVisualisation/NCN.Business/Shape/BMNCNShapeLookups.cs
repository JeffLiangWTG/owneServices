using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class BMNCNShapeLookups : AutoBMNCNShapeLookups
	{
		public BMNCNShapeLookups(AutoBMNCNShape parent)
			: base(parent)
		{
			this.parent = (BMNCNShape)parent;
		}

		readonly BMNCNShape parent;

		public IActiveBusinessObjectCollection ProcessHeaders
		{
			get
			{
				if (parent.IsDiagram)
				{
					return new ProcessJobHeaderCollection(Factory, parent.BNS_JobType);
				}
				else
				{
					return Factory.GetCachedValue("BMNCNShapeLookups.ProcessHeaders", () => new ProcessHeaderCollection(Factory));
				}
			}
		}

		public const string JobTypesCacheKey = "BMNCNShapeLookups.JobTypes";

		public CodeDescriptionPairList JobTypes
		{
			get
			{
				return Factory.GetCachedValue(JobTypesCacheKey, () =>
				{
					var filteredJobTypes = new CodeDescriptionPairList();
					var list = ObjectFactory.Get<IBMSWorkflowDescriptorList>();
					filteredJobTypes.AddRange(list.Cast<CodeDescriptionPair>().Where(x => ProcessJobHeaderProvider.SupportsPAVE(x.Code, Factory)).ToList());
					return filteredJobTypes;
				});
			}
		}

		public CodeDescriptionPairList ShapeTypes
		{
			get { return Factory.GetCachedValue<ShapeTypeList>(); }
		}

		public CodeDescriptionPairList BufferTypes
		{
			get { return Factory.GetCachedValue<BufferTypeList>(); }
		}

		public CodeDescriptionPairList ScrollPositions
		{
			get { return Factory.GetCachedValue<ScrollPositionList>(); }
		}
	}
}
