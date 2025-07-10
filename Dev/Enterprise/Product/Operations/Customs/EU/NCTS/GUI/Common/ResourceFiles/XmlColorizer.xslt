<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output method="html" indent="no"/>

	<!-- Main template to process the document -->
	<xsl:template match="/">
		<xsl:apply-templates/>
	</xsl:template>

	<!-- Template for elements -->
	<xsl:template match="*">
		<xsl:variable name="indent">
			<xsl:for-each select="ancestor::*">
				<xsl:text>&#9;</xsl:text>
			</xsl:for-each>
		</xsl:variable>
		<xsl:value-of select="$indent"/>
		<span style="color:blue;">&lt;</span><span style="color:#ac3939;"><xsl:value-of select="name()"/></span>
		<xsl:apply-templates select="@*"/>

		<xsl:choose>
			<!-- When the element has only one text node as a child -->
			<xsl:when test="count(*) = 0 and count(text()[normalize-space()]) = 1">
				<span style="color:blue;">&gt;</span><span style="font-weight:bold; color:black;"><xsl:value-of select="normalize-space(text())"/></span><span style="color:blue;">&lt;/</span><span style="color:#ac3939;"><xsl:value-of select="name()"/></span><span style="color:blue;">&gt;</span>
				<xsl:text>&#10;</xsl:text>
			</xsl:when>
			<!-- When the element has other child elements -->
			<xsl:otherwise>
				<span style="color:blue;">&gt;</span>
				<xsl:text>&#10;</xsl:text>
				<xsl:apply-templates select="node()"/>
				<xsl:value-of select="$indent"/>
				<span style="color:blue;">&lt;/</span><span style="color:#ac3939;"><xsl:value-of select="name()"/></span><span style="color:blue;">&gt;</span>
				<xsl:text>&#10;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<!-- Template for attributes -->
	<xsl:template match="@*">
		<xsl:text> </xsl:text>
		<xsl:choose>
			<xsl:when test="starts-with(name(), 'xmlns')">
				<span style="color:red;"><xsl:value-of select="name()"/></span>
				<span style="color:blue;">=&quot;</span>
				<span style="font-weight:bold; color:red;"><xsl:value-of select="."/></span>
				<span style="color:blue;">&quot;</span>
			</xsl:when>
			<xsl:otherwise>
				<span style="color:#ac3939;"><xsl:value-of select="name()"/></span>
				<span style="color:blue;">=&quot;</span>
				<span style="font-weight:bold; color:black;"><xsl:value-of select="."/></span>
				<span style="color:blue;">&quot;</span>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<!-- Template for text nodes that are not the only children -->
	<xsl:template match="text()[normalize-space()]">
		<!-- Check if this text node is the only child in the parent element -->
		<xsl:if test="not(parent::*[count(*) = 0 and count(text()[normalize-space()]) = 1])">
			<xsl:variable name="indent">
				<xsl:for-each select="ancestor::*">
					<xsl:text>&#9;</xsl:text>
				</xsl:for-each>
			</xsl:variable>
			<xsl:value-of select="$indent"/>
			<span style="font-weight:bold; color:black;"><xsl:value-of select="normalize-space(.)"/></span>
			<xsl:text>&#10;</xsl:text>
		</xsl:if>
	</xsl:template>

	<!-- Ignore text nodes that contain only whitespace -->
	<xsl:template match="text()[not(normalize-space())]"/>
</xsl:stylesheet>