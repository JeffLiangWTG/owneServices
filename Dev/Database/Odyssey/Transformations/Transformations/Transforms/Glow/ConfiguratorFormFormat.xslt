<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet
  xmlns="http://wisetechglobal.com/glow/2017/05/26/form.xsd"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:p="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
  xmlns:infrastructure="clr-namespace:CargoWise.Glow.UI.Infrastructure;assembly=CargoWise.Glow.UI.Infrastructure"
  xmlns:common="clr-namespace:CargoWise.Glow.UI.Controls;assembly=CargoWise.Glow.UI.Controls"
  xmlns:Sys="clr-namespace:System;assembly=mscorlib"
  xmlns:msxsl="urn:schemas-microsoft-com:xslt"
  exclude-result-prefixes="xsl p x infrastructure common Sys msxsl"
  version="1.0">

  <xsl:output method="xml" />

  <xsl:template match="/">
    <xsl:variable name="placeholders" select="p:UserControl/common:FormDesignableUserControl/p:Grid/infrastructure:PlaceholderExtensions.Placeholders/infrastructure:PlaceholdersContainer/infrastructure:Placeholder|p:UserControl/common:FormDesignableUserControl/infrastructure:PlaceholderExtensions.Placeholders/infrastructure:PlaceholdersContainer/infrastructure:Placeholder" />
    <xsl:variable name="id" select="$placeholders[@Name = 'DesignControlID']/@Value" />

    <xsl:variable name="extents">
      <xsl:apply-templates select="p:UserControl/common:FormDesignableUserControl/p:Grid/*[@infrastructure:DesignerExtension.ControlType]" mode="calculateExtent" />
    </xsl:variable>

    <xsl:variable name="containerWidth">
      <xsl:for-each select="msxsl:node-set($extents)/common:x">
        <xsl:sort select="@right" data-type="number" order="descending" />
        <xsl:if test="position() = 1">
          <xsl:value-of select="@right"/>
        </xsl:if>
      </xsl:for-each>
    </xsl:variable>

    <xsl:variable name="containerHeight">
      <xsl:for-each select="msxsl:node-set($extents)/common:x">
        <xsl:sort select="@bottom" data-type="number" order="descending" />
        <xsl:if test="position() = 1">
          <xsl:value-of select="@bottom"/>
        </xsl:if>
      </xsl:for-each>
    </xsl:variable>

    <form>
      <xsl:attribute name="id">
        <xsl:choose>
          <xsl:when test="$id">
            <xsl:value-of select="$id" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>00000000-0000-0000-0000-000000000042</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:attribute>
      <xsl:for-each select="p:UserControl/infrastructure:KeyFieldsExtensions.KeyFieldsCollection/x:ArrayExtension/infrastructure:KeyField">
        <keyField binding="{@BindingPath}" />
      </xsl:for-each>
      <xsl:for-each select="p:UserControl/infrastructure:AttachedRulesExtensions.AttachedRulesCollection/x:ArrayExtension/Sys:String">
        <action binding="{.}" />
      </xsl:for-each>
      <xsl:apply-templates select="$placeholders" />
      <xsl:apply-templates select="p:UserControl/common:FormDesignableUserControl/p:Grid/*[@infrastructure:DesignerExtension.ControlType]" mode="panel">
        <xsl:with-param name="elementName">control</xsl:with-param>
        <xsl:with-param name="containerWidth" select="$containerWidth" />
        <xsl:with-param name="containerHeight" select="$containerHeight" />
      </xsl:apply-templates>
      <xsl:apply-templates select="p:UserControl/common:FormDesignableUserControl/infrastructure:FormConfigurationExtensions.AdditionalPanels/infrastructure:ControlsContainer/*[@infrastructure:DesignerExtension.ControlType]" mode="panel">
        <xsl:with-param name="elementName">additional</xsl:with-param>
      </xsl:apply-templates>
    </form>
  </xsl:template>

  <xsl:template match="*" mode="panel">
    <xsl:param name="elementName" />
    <xsl:param name="containerWidth" />
    <xsl:param name="containerHeight" />
    <xsl:variable name="placeholders" select="infrastructure:PlaceholderExtensions.Placeholders/infrastructure:PlaceholdersContainer/infrastructure:Placeholder" />
    <xsl:variable name="width" select="($placeholders[@Name = 'Width']/@Value) div 50" />
    <xsl:variable name="height" select="($placeholders[@Name = 'Height']/@Value) div 42" />

    <xsl:variable name="verticalPadding">
      <xsl:choose>
        <xsl:when test="@infrastructure:DesignerExtension.ControlType = 'GRP' and $placeholders[@Name = 'HeaderDisplayMode' and @Value = 'Visible']">1</xsl:when>
        <xsl:otherwise>0</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:element name="{$elementName}">
      <xsl:attribute name="code">
        <xsl:value-of select="@infrastructure:DesignerExtension.ControlType" />
      </xsl:attribute>
      <xsl:attribute name="id">
        <xsl:value-of select="$placeholders[@Name = 'DesignControlID']/@Value" />
      </xsl:attribute>
      <xsl:call-template name="location">
        <xsl:with-param name="containerWidth" select="$containerWidth" />
        <xsl:with-param name="containerHeight" select="$containerHeight" />
        <xsl:with-param name="placeholders" select="$placeholders" />
      </xsl:call-template>
      <xsl:call-template name="binding">
        <xsl:with-param name="placeholders" select="$placeholders" />
      </xsl:call-template>
      <xsl:apply-templates select="$placeholders" />
      <xsl:apply-templates select="infrastructure:ExtendedGrid/*[@infrastructure:DesignerExtension.ControlType]" mode="field">
        <xsl:with-param name="containerWidth" select="$width" />
        <xsl:with-param name="containerHeight" select="$height - $verticalPadding" />
      </xsl:apply-templates>
      <xsl:apply-templates select="infrastructure:FormConfigurationExtensions.AdditionalFields/infrastructure:ControlsContainer/*[@infrastructure:DesignerExtension.ControlType]" mode="additionalField" />
    </xsl:element>
  </xsl:template>

  <xsl:template match="*" mode="field">
    <xsl:param name="containerWidth" />
    <xsl:param name="containerHeight" />
    <xsl:variable name="placeholders" select="infrastructure:PlaceholderExtensions.Placeholders/infrastructure:PlaceholdersContainer/infrastructure:Placeholder" />
    <control code="{@infrastructure:DesignerExtension.ControlType}" id="{$placeholders[@Name = 'DesignControlID']/@Value}">
      <xsl:call-template name="location">
        <xsl:with-param name="containerWidth" select="$containerWidth" />
        <xsl:with-param name="containerHeight" select="$containerHeight" />
        <xsl:with-param name="placeholders" select="$placeholders" />
      </xsl:call-template>
      <xsl:call-template name="binding">
        <xsl:with-param name="placeholders" select="$placeholders" />
      </xsl:call-template>
      <xsl:apply-templates select="$placeholders" />
    </control>
  </xsl:template>

  <xsl:template match="*" mode="additionalField">
    <xsl:variable name="placeholders" select="infrastructure:PlaceholderExtensions.Placeholders/infrastructure:PlaceholdersContainer/infrastructure:Placeholder" />
    <additional code="{@infrastructure:DesignerExtension.ControlType}" id="{$placeholders[@Name = 'DesignControlID']/@Value}">
      <xsl:call-template name="binding">
        <xsl:with-param name="placeholders" select="$placeholders" />
      </xsl:call-template>
      <xsl:apply-templates select="$placeholders" />
    </additional>
  </xsl:template>

  <xsl:template match="*" mode="calculateExtent">
    <xsl:variable name="p" select="infrastructure:PlaceholderExtensions.Placeholders/infrastructure:PlaceholdersContainer/infrastructure:Placeholder" />
    <xsl:variable name="left" select="($p[@Name = 'Left']/@Value) div 50" />
    <xsl:variable name="width" select="($p[@Name = 'Width']/@Value) div 50" />
    <xsl:variable name="right" select="$left + $width" />
    <xsl:variable name="top" select="($p[@Name = 'Top']/@Value) div 42" />
    <xsl:variable name="height" select="($p[@Name = 'Height']/@Value) div 42" />
    <xsl:variable name="bottom" select="$top + $height" />
    <common:x>
      <xsl:attribute name="right">
        <xsl:choose>
          <xsl:when test="$right >= 0">
            <xsl:value-of select="$right" />
          </xsl:when>
          <xsl:otherwise>0</xsl:otherwise>
        </xsl:choose>
      </xsl:attribute>
      <xsl:attribute name="bottom">
        <xsl:choose>
          <xsl:when test="$bottom >= 0">
            <xsl:value-of select="$bottom" />
          </xsl:when>
          <xsl:otherwise>0</xsl:otherwise>
        </xsl:choose>
      </xsl:attribute>
    </common:x>
  </xsl:template>

  <xsl:template name="location">
    <xsl:param name="containerWidth" />
    <xsl:param name="containerHeight" />
    <xsl:param name="placeholders" />

    <xsl:variable name="left" select="($placeholders[@Name = 'Left']/@Value) div 50" />
    <xsl:variable name="top" select="($placeholders[@Name = 'Top']/@Value) div 42" />
    <xsl:variable name="width" select="($placeholders[@Name = 'Width']/@Value) div 50" />
    <xsl:variable name="height" select="($placeholders[@Name = 'Height']/@Value) div 42" />
    <xsl:variable name="anchor" select="$placeholders[@Name = 'Anchor']/@Value" />

    <xsl:if test="contains($anchor, 'Left')">
      <xsl:attribute name="left">
        <xsl:value-of select="$left" />
      </xsl:attribute>
    </xsl:if>

    <xsl:if test="contains($anchor, 'Top')">
      <xsl:attribute name="top">
        <xsl:value-of select="$top" />
      </xsl:attribute>
    </xsl:if>

    <xsl:if test="not(contains($anchor, 'Left')) or not(contains($anchor, 'Right'))">
      <xsl:attribute name="width">
        <xsl:value-of select="$width" />
      </xsl:attribute>
    </xsl:if>

    <xsl:if test="not(contains($anchor, 'Top')) or not(contains($anchor, 'Bottom'))">
      <xsl:attribute name="height">
        <xsl:value-of select="$height" />
      </xsl:attribute>
    </xsl:if>

    <xsl:if test="contains($anchor, 'Right')">
      <xsl:attribute name="right">
        <xsl:choose>
          <xsl:when test="$containerWidth - $left - $width &gt; 0">
            <xsl:value-of select="$containerWidth - $left - $width" />
          </xsl:when>
          <xsl:otherwise>0</xsl:otherwise>
        </xsl:choose>
      </xsl:attribute>
    </xsl:if>

    <xsl:if test="contains($anchor, 'Bottom')">
      <xsl:attribute name="bottom">
        <xsl:choose>
          <xsl:when test="$containerHeight - $top - $height &gt; 0">
            <xsl:value-of select="$containerHeight - $top - $height" />
          </xsl:when>
          <xsl:otherwise>0</xsl:otherwise>
        </xsl:choose>
      </xsl:attribute>
    </xsl:if>
  </xsl:template>

  <xsl:template name="binding">
    <xsl:param name="placeholders" />

    <xsl:variable name="binding" select="$placeholders[@Name = 'BindingPath']/@Value" />

    <xsl:if test="$binding">
      <xsl:attribute name="binding">
        <xsl:value-of select="$binding" />
      </xsl:attribute>
    </xsl:if>
  </xsl:template>

  <xsl:template match="infrastructure:Placeholder[@Name = 'BindingPath']" />
  <xsl:template match="infrastructure:Placeholder[@Name = 'BindingMode']" />
  <xsl:template match="infrastructure:Placeholder[@Name = 'DataType']" />
  <xsl:template match="infrastructure:Placeholder[@Name = 'EntityName']" />
  <xsl:template match="infrastructure:Placeholder[@Name = 'Left']" />
  <xsl:template match="infrastructure:Placeholder[@Name = 'Top']" />
  <xsl:template match="infrastructure:Placeholder[@Name = 'Width']" />
  <xsl:template match="infrastructure:Placeholder[@Name = 'Height']" />
  <xsl:template match="infrastructure:Placeholder[@Name = 'DesignControlID']" />
  <xsl:template match="infrastructure:Placeholder[@Name = 'Anchor']" />
  <xsl:template match="infrastructure:Placeholder">
    <placeholder name="{@Name}" value="{@Value}">
      <xsl:if test="@ResKey">
        <xsl:attribute name="resid">
          <xsl:value-of select="@ResKey" />
        </xsl:attribute>
      </xsl:if>
    </placeholder>
  </xsl:template>
</xsl:stylesheet>
