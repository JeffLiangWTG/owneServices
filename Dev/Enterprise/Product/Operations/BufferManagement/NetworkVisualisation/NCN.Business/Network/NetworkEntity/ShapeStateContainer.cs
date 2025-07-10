using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.Integration;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Network.NetworkEntity
{
	public static class ShapeStateContainer
	{
		[ThreadSafe(ThreadSafeAttribute.Mechanism.Lock)]
		static ConcurrentBag<ShapeState> States = new ConcurrentBag<ShapeState>();
		[ThreadSafe(ThreadSafeAttribute.Mechanism.Lock)]
		static ConcurrentBag<LinkShapeState> Links = new ConcurrentBag<LinkShapeState>();
		[ThreadSafe(ThreadSafeAttribute.Mechanism.Lock)]
		static bool clearAfterFirstGet = false;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccess", Justification = "Baseline")]
		static void AddNewLinkIfBothSidesAreSelected(LinkShapeState linkState, IEnumerable<ShapeState> rootStates)
		{
			var isNotDupllicatedLink = !Links.Any(link => link.From == linkState.From && link.To == linkState.To);
			if (BothSidesAreSelected(linkState, rootStates) && isNotDupllicatedLink)
			{
				Links.Add(linkState);
			}
		}

		static ShapeState CreateShapeState(INetworkEntity entity)
		{
			var result = new ShapeState()
			{
				Name = entity?.Name,
				Height = entity.Height,
				Width = entity.Width,
				AdditionalDetails = entity.AdditionalDetail,
				ForeColor = entity.ForeColor,
				ShapeNotes = entity?.AdditionalDetail,
				BackColor = entity.BackColor,
				CompletionCriteria = entity?.CompletionCriteria,
				JobName = entity?.JobName,
				LayoutData = entity.AsShape().BNS_LayoutData,
				JobType = entity.AsShape().BNS_JobType,
				RelatedEntityID = entity.AsShape().BNS_RelatedEntityID.ToString(),
				RelatedEntityTableCode = entity.AsShape().BNS_RelatedEntityTableCode,
				ShapeType = entity.AsShape().ShapeType,
				Style = entity.AsShape().BNS_BNT_Style.ToString(),
				Status = entity.AsShape().BNS_Status,
				PK = entity.AsShape().PK.ToString(),
				BNS_RelatedEntityTableCode = entity.AsShape().BNS_RelatedEntityTableCode,
				BNS_RelatedEntityID = entity.AsShape().BNS_RelatedEntityID.ToString()
			};

			//Creating children of a shape state
			entity.Children.ForEach(child => result.Children.Add(CreateShapeState(child)));

			return result;
		}

		static void CreateLinkShapeState(INetworkEntity entity, IEnumerable<ShapeState> rootStates)
		{
			entity.AsShape().AllAttachments.ForEach(attachment =>
			{
				var includesFromAndtoShape = (attachment.FromShape != null) && (attachment.ToShape != null);
				if (attachment.BNA_IsHidden == false && includesFromAndtoShape)
				{
					var linkState = new LinkShapeState()
					{
						From = attachment.FromShape.PK.ToString(),
						To = attachment.ToShape.PK.ToString(),
						FPProcessHeaderLink = attachment.BNA_FP_ProcessHeaderLink.ToString(),
						BNAType = attachment.BNA_Type
					};
					AddNewLinkIfBothSidesAreSelected(linkState, rootStates);
				}
			});

			entity.Children.ForEach(child => CreateLinkShapeState(child, rootStates));
		}

		static bool DoesExist(IEnumerable<ShapeState> shapeStates, string pk)
		{
			foreach (var shapeState in shapeStates)
			{
				if (shapeState.PK == pk)
				{
					return true;
				}

				foreach (var child in shapeState.Children)
				{
					if (DoesExist(new List<ShapeState> { child }, pk))
					{
						return true;
					}
				}
			}
			return false;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccess", Justification = "Baseline")]
		public static bool Set(IEnumerable<INetworkEntity> nodes, bool clearAfterGet)
		{
			Clear();
			nodes.Where(n => n.ShapeType != ShapeTypeList.Codes.Buffer)
				.ForEach(entity =>
			{
				//Creating shape states
				var shapeState = CreateShapeState(entity);

				States.Add(shapeState);

				//Creating links
				CreateLinkShapeState(entity, States.ToList());
			});
			clearAfterFirstGet = clearAfterGet;

			return true;
		}

		static bool BothSidesAreSelected(LinkShapeState linkState, IEnumerable<ShapeState> rootStates)
		{
			var fromExists = rootStates.Any(state => DoesExist(rootStates, linkState.From));
			var toExists = rootStates.Any(state => DoesExist(rootStates, linkState.To));

			return fromExists && toExists;
		}

		public static (IEnumerable<ShapeState>, IEnumerable<LinkShapeState>) Get(bool keepData = false)
		{
			var states = new List<ShapeState>();
			var links = new List<LinkShapeState>();
			links.AddRange(Links);
			states.AddRange(States);

			if (clearAfterFirstGet && !keepData)
			{
				Clear();
			}

			return (states, links);
		}

		public static void Clear()
		{
			States = new ConcurrentBag<ShapeState>();
			Links = new ConcurrentBag<LinkShapeState>();
		}

		public static int GetShapeCount()
		{
			var (states, links) = Get(true);
			return states.Count();
		}
	}
}
